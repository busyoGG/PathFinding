using UnityEngine;

namespace AStar
{
    public class AStarData
    {
        // public Vector2 pos;

        public int index;
        
        public int x;

        public int y;

        /// <summary>
        /// 移动权重
        /// </summary>
        public int g;

        /// <summary>
        /// 曼哈顿距离
        /// </summary>
        public int h;

        /// <summary>
        /// 总估值
        /// </summary>
        public int f
        {
            get
            {
                return g + h;
            }
        }

        public bool enable;

        public AStarData parent;
    }
}
