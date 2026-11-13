using NavigationDJIA.World;

namespace GrupoR
{
    /// <summary>
    /// Estado del agente para la Tabla Q. No usa posiciones absolutas, sólo información
    /// relativa y local, para que generalice a cualquier distribución de muros:
    ///   - DirX, DirY: dirección (signo -1/0/1) en la que está el enemigo respecto al agente.
    ///   - DistBucket: distancia Manhattan al enemigo discretizada en 5 rangos
    ///                 (0: <=1, 1: 2, 2: 3-4, 3: 5-7, 4: >=8).
    ///   - WallUp/Down/Left/Right: si la casilla vecina en cada dirección está bloqueada
    ///                 (muro o fuera del tablero).
    /// Total: 9 direcciones x 5 distancias x 16 combinaciones de muros = 720 estados posibles.
    /// </summary>
    public sealed class QState
    {
        public int DirX { get; }
        public int DirY { get; }
        public int Distancia { get; }
        public bool WallUp { get; }
        public bool WallDown { get; }
        public bool WallLeft { get; }
        public bool WallRight { get; }

        public QState(CellInfo agent, CellInfo other, WorldInfo worldInfo)
        {
            int dx = other.x - agent.x;
            int dy = other.y - agent.y;

            DirX = Sign(dx);
            DirY = Sign(dy);
            Distancia = Rango(System.Math.Abs(dx) + System.Math.Abs(dy));

            WallUp = IsBlocked(worldInfo, agent.x, agent.y + 1);
            WallDown = IsBlocked(worldInfo, agent.x, agent.y - 1);
            WallLeft = IsBlocked(worldInfo, agent.x - 1, agent.y);
            WallRight = IsBlocked(worldInfo, agent.x + 1, agent.y);
        }

        /// <summary>
        /// Clave única del estado, p. ej. "1,-1|2|0100".
        /// Los bits de muros siguen el orden Up, Down, Left, Right.
        /// </summary>
        public string ToKey()
        {
            return $"{DirX},{DirY}|{Distancia}|{B(WallUp)}{B(WallDown)}{B(WallLeft)}{B(WallRight)}";
        }

        /// <summary>Casilla no transitable: fuera del tablero o con muro.</summary>
        public static bool IsBlocked(WorldInfo worldInfo, int x, int y)
        {
            if (x < 0 || y < 0 || x >= worldInfo.WorldSize.x || y >= worldInfo.WorldSize.y)
                return true;

            return !worldInfo[x, y].Walkable;
        }

        private static int Sign(int v) => v > 0 ? 1 : (v < 0 ? -1 : 0);

        private static int Rango(int d)
        {
            if (d <= 1) return 0;
            if (d == 2) return 1;
            if (d <= 4) return 2;
            if (d <= 7) return 3;
            return 4;
        }

        private static char B(bool b) => b ? '1' : '0';
    }
}
