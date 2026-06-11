using NavigationDJIA.World;
using System;

/// <summary>
/// TODO(alumno):
/// Define el "estado" que usará la Tabla Q para identificar cada situación del agente.
/// 
/// El estado debe contener toda la información necesaria para que el agente pueda
/// tomar decisiones informadas. Tú decides qué características incluir según lo
/// que consideres relevante para resolver el problema.
/// 
/// Ejemplos típicos de información que puede formar un estado:
///   - Posición del agente en la grid.
///   - Posición del otro personaje (enemigo).
///   - Distancia relativa entre agente y enemigo.
///   - Si hay muros en direcciones cercanas.
///   - Cualquier otro dato que consideres útil.
/// 
/// En este ejercicio te damos un ejemplo simple basado únicamente en las posiciones
/// del agente y del oponente. Puedes usarlo tal cual o ampliarlo.
/// 
/// IMPORTANTE: 
///  El estado debe poder convertirse a una clave única (string) mediante ToKey(),
///  ya que esa clave se usará como índice en la TablaQ y en el archivo CSV.
/// </summary>

namespace GrupoN
{
    public sealed class QState
    {
        public int AgentX { get; }
        public int AgentY { get; }
        public int OtherX { get; }
        public int OtherY { get; }

        public QState(CellInfo agent, CellInfo other)
        {
            AgentX = agent.x;
            AgentY = agent.y;
            OtherX = other.x;
            OtherY = other.y;
        }

        public string ToKey()
        {
            int dx = OtherX - AgentX;
            int dy = OtherY - AgentY;

            int manhattan = Math.Abs(dx) + Math.Abs(dy);

            // Dirección relativa
            int dirX = Math.Sign(dx);
            int dirY = Math.Sign(dy);

            // Distancia categorizada para reducir la cantidad de estados sin perder mucha información
            int distBucket =
                manhattan <= 2 ? 0 :
                manhattan <= 5 ? 1 :
                manhattan <= 9 ? 2 :
                manhattan <= 14 ? 3 : 4;

            // Forma de persecucióna basada en alineación simple
            int alignment =
                (dx == 0 || dy == 0) ? 1 : 0;

            
            // Si está muy cerca significa peligro alto
            int dangerZone =
                manhattan <= 2 ? 3 :
                manhattan <= 4 ? 2 :
                manhattan <= 7 ? 1 : 0;
        
            // Combinamos todas las características en una clave única
            return $"{dirX},{dirY},{distBucket},{alignment},{dangerZone}";
        }
    }
}