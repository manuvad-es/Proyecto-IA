using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;

namespace GrupoR
{
    public class QMindTester : IQMind
    {
        private WorldInfo _worldInfo;
        private QTableStorage _qStorage;
        private QTable _qTable;

        public void Initialize(WorldInfo worldInfo)
        {
            _worldInfo = worldInfo;

            _qStorage = new QTableStorage("TablaQ.csv");
            _qTable = new QTable(_qStorage);
        }

        public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
        {
            string stateKey = BuildStateKey(currentPosition, otherPosition, _worldInfo);

            QAction bestAction = _qTable.GetBestAction(stateKey);

            CellInfo nextPosition = ApplyAction(currentPosition, bestAction);

            return nextPosition;
        }

        // Misma cabecera e implementación que en QMindTrainer
        private string BuildStateKey(CellInfo agent, CellInfo other, WorldInfo _worldInfo)
        {
            var state = new QState(agent, other, _worldInfo);
            return state.ToKey();
        }

        // Si la acción lleva a un muro o fuera del tablero, el agente se queda donde está
        // (mismo comportamiento que en el entrenamiento).
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
    }
}
