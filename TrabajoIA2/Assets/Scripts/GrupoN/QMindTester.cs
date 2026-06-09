using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;

namespace GrupoN
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
            string stateKey = BuildStateKey(currentPosition, otherPosition);

            QAction bestAction = _qTable.GetBestAction(stateKey);

            CellInfo nextPosition = ApplyAction(currentPosition, bestAction);

            return nextPosition;
        }

        private string BuildStateKey(CellInfo agent, CellInfo other)
        {
            var state = new QState(agent, other);
            return state.ToKey();
        }

        private CellInfo ApplyAction(CellInfo agentCell, QAction action)
        {
            int nx = agentCell.x;
            int ny = agentCell.y;

            switch (action)
            {
                case QAction.Up:
                    ny += 1;
                    break;

                case QAction.Down:
                    ny -= 1;
                    break;

                case QAction.Right:
                    nx += 1;
                    break;

                case QAction.Left:
                    nx -= 1;
                    break;

                case QAction.Stay:
                    return agentCell;
            }

            CellInfo destination = _worldInfo[nx, ny];

            // Si la celda no existe o no es caminable, nos quedamos donde estamos
            if (destination == null || !destination.Walkable)
                return agentCell;

            return destination;
        }
    }
}