using NavigationDJIA.World;

namespace GrupoR
{
    // Estado del agente, dónde esta el enemigo, la distancia y si hay muros o está el límite del mapa cerca
    public sealed class QState
    {
        public int DirX { get; }
        public int DirY { get; }
        public int Distancia { get; }
        public bool WallUp { get; }
        public bool WallDown { get; }
        public bool WallLeft { get; }
        public bool WallRight { get; }

        // Cálculo de los valores del estado
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

        // Clave única del estado por ejemplo: "1,-1|2|0100"
        public string ToKey()
        {
            return $"{DirX},{DirY}|{Distancia}|{B(WallUp)}{B(WallDown)}{B(WallLeft)}{B(WallRight)}";
        }

        // Comprobar fuera del tablero o casilla con muro
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
