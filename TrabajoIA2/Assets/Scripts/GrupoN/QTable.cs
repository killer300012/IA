using System;
using QMind;

namespace GrupoN
{
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

        /// <summary>
        /// TODO(alumno):
        /// Devuelve el valor Q(s, a) correspondiente al estado y acción indicados.
        /// 
        /// Pasos recomendados:
        ///  1. Asegúrate de que el estado existe llamando a EnsureState(stateKey).
        ///  2. Convierte la acción en un índice del array:
        ///        int index = (int)action;
        ///  3. Devuelve el valor almacenado en:
        ///        _storage.Data[stateKey][index]
        /// </summary>
        public float GetQ(string stateKey, QAction action)
        {
            // Nos aseguramos de que el estado existe en la tabla
            EnsureState(stateKey);

            // Convertimos la acción a índice
            int index = (int)action;

            // Devolvemos Q(s,a)
            return _storage.Data[stateKey][index];
        }

        /// <summary>
        /// TODO(alumno):
        /// Asigna el valor Q(s, a) para el estado y acción indicados.
        /// 
        /// Pasos recomendados:
        ///  1. Asegúrate de que el estado existe llamando a EnsureState(stateKey).
        ///  2. Convierte la acción en un índice del array:
        ///        int index = (int)action;
        ///  3. Guarda el valor recibido en:
        ///        _storage.Data[stateKey][index] = value;
        /// </summary>
        public void SetQ(string stateKey, QAction action, float value)
        {
            // Nos aseguramos de que el estado existe
            EnsureState(stateKey);

            // Convertimos la acción a índice
            int index = (int)action;

            // Guardamos el nuevo valor Q(s,a)
            _storage.Data[stateKey][index] = value;
        }

        /// <summary>
        /// TODO(alumno):
        /// Devuelve el valor máximo max_a Q(s, a) para el estado indicado.
        /// 
        /// Este método se usa en la actualización de Q-Learning:
        ///   maxQNext = GetMaxQ(nextStateKey)
        /// 
        /// Pasos recomendados:
        ///  1. Asegúrate de que el estado existe llamando a EnsureState(stateKey).
        ///  2. Obtén el array de Q-values:
        ///        var qValues = _storage.Data[stateKey];
        ///  3. Recorre el array buscando el valor máximo y devuélvelo.
        /// </summary>
        public float GetMaxQ(string stateKey)
        {
            // Nos aseguramos de que el estado existe
            EnsureState(stateKey);

            // Obtenemos todos los valores Q asociados al estado
            float[] qValues = _storage.Data[stateKey];

            // Inicializamos el máximo con el primer valor
            float max = qValues[0];

            // Recorremos el resto buscando el mayor
            for (int i = 1; i < qValues.Length; i++)
            {
                if (qValues[i] > max)
                {
                    max = qValues[i];
                }
            }

            // Devolvemos max_a Q(s,a)
            return max;
        }

        /// <summary>
        /// TODO(alumno):
        /// Devuelve la mejor acción para el estado indicado:
        ///    argmax_a Q(s, a)
        /// 
        /// Este método se usa para:
        ///   - Política greedy (explotar lo aprendido).
        ///   - Parte "explotar" de la política ε-greedy.
        /// 
        /// Pasos recomendados:
        ///  1. Asegúrate de que el estado existe llamando a EnsureState(stateKey).
        ///  2. Obtén el array de Q-values:
        ///        var qValues = _storage.Data[stateKey];
        ///  3. Recorre el array buscando el índice del valor máximo.
        ///  4. Convierte ese índice a QAction:
        ///        return (QAction)bestIndex;
        /// </summary>
        public QAction GetBestAction(string stateKey)
        {
            // Nos aseguramos de que el estado existe
            EnsureState(stateKey);

            // Recuperamos los valores Q del estado
            float[] qValues = _storage.Data[stateKey];

            // Inicializamos la mejor acción con la primera
            int bestIndex = 0;
            float bestValue = qValues[0];

            // Buscamos el índice asociado al valor máximo
            for (int i = 1; i < qValues.Length; i++)
            {
                if (qValues[i] > bestValue)
                {
                    bestValue = qValues[i];
                    bestIndex = i;
                }
            }

            // Convertimos el índice encontrado a QAction
            return (QAction)bestIndex;
        }

        public void SaveToCsv()
        {
            _storage.Save();
        }
    }
}