using System;
using QMind;

namespace GrupoR
{
    // Tabla de calidad Q(s, a). Cada fila del diccionario es un estado del ToKey
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

        // Devuelve Q(s, a)
        public float GetQ(string stateKey, QAction action)
        {
            EnsureState(stateKey);
            return _storage.Data[stateKey][(int)action];
        }

        // Asigna Q(s, a) el valor
        public void SetQ(string stateKey, QAction action, float value)
        {
            EnsureState(stateKey);
            _storage.Data[stateKey][(int)action] = value;
        }

        // Devuelve max_a Q(s, a)
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

        // Devuelve argmax_a Q(s, a). En caso de ser iguales, la primera acción
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
