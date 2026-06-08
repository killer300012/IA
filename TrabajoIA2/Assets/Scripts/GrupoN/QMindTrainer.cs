using System;
using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;

namespace GrupoN
{
    public class QMindTrainer : IQMindTrainer
    {
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
            _qTable.SaveToCsv();

            OnEpisodeFinished?.Invoke(this, EventArgs.Empty);

            if (_params.episodes > 0 && CurrentEpisode >= _params.episodes)
            {
                return;
            }

            StartNewEpisode();
        }

        public void DoStep(bool train)
        {
            // Estado actual del agente
            string stateKey = BuildStateKey(_agentPosition, _otherPosition);

            // Seleciona la acción a realizar
            QAction action = ChooseAction(stateKey, train);

            // Nuevos estados del agente y del oponente
            CellInfo newAgentPos = ApplyAction(_agentPosition, action);
            CellInfo newOtherPos = MoveOpponent(_otherPosition, newAgentPos.Walkable ? newAgentPos : _agentPosition);
            
            // Nuevo estado del agente
            string nextStateKey = BuildStateKey(newAgentPos, newOtherPos);
            
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

        private string BuildStateKey(CellInfo agent, CellInfo other)
        {
            var state = new QState(agent, other);
            return state.ToKey();
        }

        /// <summary>
        /// Ejemplo orientativo:
        ///    - Si train == false, puedes usar la mejor acción.
        ///    - Si train == true, con probabilidad epsilon elegir acción aleatoria,
        ///      y con probabilidad 1-epsilon la mejor según _qTable.GetBestAction(stateKey).
        /// </summary>
        private QAction ChooseAction(string stateKey, bool train)
        {
            if (!train)
                return _qTable.GetBestAction(stateKey);

            if (_random.NextDouble() < _params.epsilon)
            {
                Array actions = Enum.GetValues(typeof(QAction));
                return (QAction)actions.GetValue(_random.Next(actions.Length));
            }

            return _qTable.GetBestAction(stateKey);
        }

        /// <summary>
        /// Actualización de Q-Learning:
        /// Q(s,a) = (1 - alpha) * Q(s,a) + alpha * (reward + gamma * max_a' Q(s',a')).
        /// Usa _qTable.GetQ, _qTable.SetQ y _qTable.GetMaxQ.
        /// </summary>
        private void UpdateQ(string stateKey,QAction action,float reward,string nextStateKey)
        {
            float oldQ = _qTable.GetQ(stateKey, action);

            float maxQNext = _qTable.GetMaxQ(nextStateKey);

            float target =
                reward + _params.gamma * maxQNext;

            float newQ =
                (1 - _params.alpha) * oldQ
                + _params.alpha * target;

            _qTable.SetQ(stateKey, action, newQ);
        }

        /// <summary>
        /// Función de recompensa.
        /// Ejemplo orientativo:
        ///   si agent == other -> recompensa positiva grande (captura)
        ///   si no -> pequeña penalización negativa por cada paso.
        /// </summary>
        private float ComputeReward(CellInfo agent, CellInfo other)
        {
            if (agent == other)
                return -100f;

            int distance =
                Math.Abs(agent.x - other.x)
              + Math.Abs(agent.y - other.y);

            return distance * 0.1f;
        }

        /// <summary>
        /// Condición de final de episodio.
        /// Lo más simple: cuando agente y oponente están en la misma celda.
        /// También puedes definir una probabilidad para el parámetro v visto en clase.
        /// </summary>
        private bool IsTerminalState(CellInfo agent,CellInfo other)
        {
            return agent == other;
        }


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

            // Obtener la celda candidata y validar si es caminable
            try
            {
                var candidate = _worldInfo[nx, ny];
                // Si la celda es nula o no caminable, permanecer en la celda actual.
                if (candidate == null || !candidate.Walkable)
                    return agentCell;

                return candidate;
            }
            catch (IndexOutOfRangeException)
            {
                // Fuera de límites es permanecer en la celda actual.
                return agentCell;
            }
            catch (Exception)
            {
                // Cualquier otro error significa permanecer en la celda actual.
                return agentCell;
            }
        }


        private CellInfo MoveOpponent(CellInfo opponent, CellInfo target)
        {
            // El oponente se mueve hacia el agente usando el algoritmo de navegación.
            var path = _navigationAlgorithm.GetPath(opponent, target, 1);
            // Si el path es null o vacío, el oponente se queda en su posición actual.
            if (path == null)
                return opponent;
            // Si el path tiene al menos un paso, el oponente se mueve a la primera celda del path.
            if (path.Length > 0)
                return path[0];

            return opponent;
        }
        #endregion
    }
}
