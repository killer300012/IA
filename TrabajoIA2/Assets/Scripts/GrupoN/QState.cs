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
            // Ejemplo de estado basado en la dirección y distancia relativa entre el agente y el oponente.
            int dx = OtherX - AgentX;
            int dy = OtherY - AgentY;
            // Dirección normalizada a -1, 0 o 1
            int dirX = Math.Sign(dx);
            int dirY = Math.Sign(dy);

            // Distancia categorizada en buckets (0-2, 3-5, 6-10, >10)
            int distance = Math.Abs(dx) + Math.Abs(dy);

            int bucket;

            // Categorizamos la distancia en buckets para reducir el número de estados posibles
            if (distance <= 2)
                bucket = 0;
            else if (distance <= 4)
                bucket = 1;
            else if (distance <= 6)
                bucket = 2;
            else if (distance <= 8)
                bucket = 3;
            else if (distance <= 12)
                bucket = 4;
            else
                bucket = 5;

            // La clave del estado combina la dirección relativa y el bucket de distancia
            return $"{dirX},{dirY},{bucket}";
        }
    }
}