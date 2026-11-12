using System;
using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;
using UnityEngine;

namespace GrupoA
{
    public class QMindTrainer : IQMindTrainer
    {
        private const float MinEpsilon = 0.05f;

        private QMindTrainerParams _params;
        private WorldInfo _worldInfo;
        INavigationAlgorithm _navigationAlgorithm;

        private QTableStorage _qStorage;
        private QTable _qTable;

        private CellInfo _agentPosition;
        private CellInfo _otherPosition;

        private float _return;
        private float _returnAveraged;
        private System.Random _random = new System.Random();

        #region IQMindTrainer implementation

        public CellInfo AgentPosition => _agentPosition;
        public CellInfo OtherPosition => _otherPosition;

        public int CurrentEpisode { get; private set; }
        public int CurrentStep { get; private set; }

        public float Return => _return;
        public float ReturnAveraged => _returnAveraged;

        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        #endregion

        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            _params = qMindTrainerParams;
            _worldInfo = worldInfo;
            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(worldInfo);

            _qStorage = new QTableStorage("TablaQ.csv");
            _qTable = new QTable(_qStorage);

            CurrentEpisode = 0;
            StartNewEpisode();
        }

        private void StartNewEpisode()
        {
            CurrentEpisode++;
            CurrentStep = 0;
            _return = 0f;
            _returnAveraged = 0f;

            _agentPosition = _worldInfo.RandomCell();
            _otherPosition = _worldInfo.RandomCell();

            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        private void EndEpisode()
        {
            bool lastEpisode = _params.episodes > 0 && CurrentEpisode >= _params.episodes;

            // Guardamos la tabla cada N episodios y al terminar el entrenamiento
            if (lastEpisode || CurrentEpisode % _params.episodesBetweenSaves == 0)
            {
                _qTable.SaveToCsv();
            }

            OnEpisodeFinished?.Invoke(this, EventArgs.Empty);

            if (lastEpisode)
            {
                return;
            }

            StartNewEpisode();
        }

        public void DoStep(bool train)
        {
            // Estado actual del agente
            string stateKey = BuildStateKey(_agentPosition, _otherPosition, _worldInfo);

            // Seleciona la acción a realizar
            QAction action = ChooseAction(stateKey, train);

            // Nuevos estados del agente y del oponente
            CellInfo newAgentPos = ApplyAction(_agentPosition, action);
            CellInfo newOtherPos = MoveOpponent(_otherPosition, newAgentPos);

            // Nuevo estado del agente
            string nextStateKey = BuildStateKey(newAgentPos, newOtherPos, _worldInfo);

            // Calcula la recompensa
            float reward = ComputeReward(newAgentPos, newOtherPos);

            if (train)
            {
                UpdateQ(stateKey, action, reward, nextStateKey);
            }

            // actualiza las posiciones
            _agentPosition = newAgentPos;
            _otherPosition = newOtherPos;

            // Actualizamos estadísticas de recompensas
            CurrentStep++;
            _return += reward;
            _returnAveraged = (_returnAveraged * (CurrentStep - 1) + reward) / CurrentStep;

            // Comprobación de si estamos en el fin de episodio
            if (IsTerminalState(_agentPosition, _otherPosition))
            {
                EndEpisode();
            }
        }

        #region Parte a implementar por el alumno

        private string BuildStateKey(CellInfo agent, CellInfo other, WorldInfo _worldInfo)
        {
            var state = new QState(agent, other, _worldInfo);
            return state.ToKey();
        }

        /// <summary>
        /// Política epsilon-greedy: con probabilidad epsilon acción aleatoria (explorar),
        /// si no, la mejor acción según la tabla (explotar).
        /// </summary>
        private QAction ChooseAction(string stateKey, bool train)
        {
            if (!train)
                return _qTable.GetBestAction(stateKey);

            if (_random.NextDouble() < CurrentEpsilon())
                return (QAction)_random.Next(5);

            return _qTable.GetBestAction(stateKey);
        }

        /// <summary>
        /// Epsilon decae linealmente desde _params.epsilon hasta MinEpsilon
        /// a lo largo de los episodios de entrenamiento.
        /// </summary>
        private float CurrentEpsilon()
        {
            if (_params.episodes <= 0)
                return _params.epsilon;

            float progress = (float)(CurrentEpisode - 1) / _params.episodes;
            return Mathf.Max(MinEpsilon, _params.epsilon * (1f - progress));
        }

        /// <summary>
        /// Q(s,a) = (1 - alpha) * Q(s,a) + alpha * (reward + gamma * max_a' Q(s',a')).
        /// </summary>
        private void UpdateQ(string stateKey, QAction action, float reward, string nextStateKey)
        {
            float oldQ = _qTable.GetQ(stateKey, action);
            float maxQNext = _qTable.GetMaxQ(nextStateKey);
            float target = reward + _params.gamma * maxQNext;
            float newQ = (1 - _params.alpha) * oldQ + _params.alpha * target;
            _qTable.SetQ(stateKey, action, newQ);
        }

        /// <summary>
        /// -100 si el enemigo captura al agente.
        /// Si sobrevive: +1 más 0.1 por casilla de distancia al enemigo (hasta 10),
        /// de modo que la recompensa disminuye cuanto más cerca está el enemigo.
        /// Si el enemigo está a 1 casilla, se restan 5 puntos (zona de peligro).
        /// </summary>
        private float ComputeReward(CellInfo agent, CellInfo other)
        {
            if (agent.Equals(other))
                return -100f;

            int distance = Math.Abs(agent.x - other.x) + Math.Abs(agent.y - other.y);
            float reward = 1f + 0.1f * Math.Min(distance, 10);

            if (distance <= 2) reward -= 2f;

            return reward;
        }

        /// <summary>
        /// El episodio termina cuando el enemigo alcanza la casilla del agente.
        /// </summary>
        private bool IsTerminalState(CellInfo agent, CellInfo other)
        {
            return agent.Equals(other);
        }

        /// <summary>
        /// Si la acción lleva a un muro o fuera del tablero, el agente se queda en su casilla.
        /// </summary>
        private CellInfo ApplyAction(CellInfo agentCell, QAction action)
        {
            int nx = agentCell.x;
            int ny = agentCell.y;

            switch (action)
            {
                case QAction.Up:    ny += 1; break;
                case QAction.Down:  ny -= 1; break;
                case QAction.Right: nx += 1; break;
                case QAction.Left:  nx -= 1; break;
                case QAction.Stay:  return agentCell;
            }

            if (QState.IsBlocked(_worldInfo, nx, ny))
                return agentCell;

            return _worldInfo[nx, ny];
        }

        private CellInfo MoveOpponent(CellInfo opponent, CellInfo target)
        {
            var path = _navigationAlgorithm.GetPath(opponent, target, 1);
            if (path.Length > 0)
                return path[0];

            return opponent;
        }
        #endregion
    }
}
