using System;
using QMind;

namespace GrupoR
{
    /// <summary>
    /// Tabla de calidad Q(s, a). Cada fila del diccionario es un estado (clave de QState.ToKey())
    /// y el array de floats contiene un valor Q por acción, indexado por (int)QAction.
    /// </summary>
    public class QTable
    {
        private readonly QTableStorage _storage;
        private readonly string[] _actionNames;

        public QTable(QTableStorage storage)
        {
            _storage = storage;
            _actionNames = Enum.GetNames(typeof(QAction));
        }

        private void EnsureState(string stateKey)
        {
            if (!_storage.Data.ContainsKey(stateKey))
            {
                _storage.Data[stateKey] = new float[_actionNames.Length];
            }
        }

        /// <summary>Devuelve Q(s, a).</summary>
        public float GetQ(string stateKey, QAction action)
        {
            EnsureState(stateKey);
            return _storage.Data[stateKey][(int)action];
        }

        /// <summary>Asigna Q(s, a) = value.</summary>
        public void SetQ(string stateKey, QAction action, float value)
        {
            EnsureState(stateKey);
            _storage.Data[stateKey][(int)action] = value;
        }

        /// <summary>Devuelve max_a Q(s, a).</summary>
        public float GetMaxQ(string stateKey)
        {
            EnsureState(stateKey);
            float[] qValues = _storage.Data[stateKey];

            float max = qValues[0];
            for (int i = 1; i < qValues.Length; i++)
            {
                if (qValues[i] > max) max = qValues[i];
            }
            return max;
        }

        /// <summary>Devuelve argmax_a Q(s, a). En caso de empate, la primera acción.</summary>
        public QAction GetBestAction(string stateKey)
        {
            EnsureState(stateKey);
            float[] qValues = _storage.Data[stateKey];

            int bestIndex = 0;
            for (int i = 1; i < qValues.Length; i++)
            {
                if (qValues[i] > qValues[bestIndex]) bestIndex = i;
            }
            return (QAction)bestIndex;
        }

        public void SaveToCsv()
        {
            _storage.Save();
        }
    }
}
