using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace AStar
{
    public class AStarManager : Singleton<AStarManager>
    {
        private SortedSet<AStarData> _openList = new SortedSet<AStarData>(Comparer<AStarData>.Create((a, b) =>
        {
            int compare = a.f.CompareTo(b.f);
            if (compare == 0)
                compare = a.h.CompareTo(b.h);
            if(compare == 0)
                compare = a.index.CompareTo(b.index);
            return compare;
        }));

        private HashSet<AStarData> _closeList = new HashSet<AStarData>();

        private AStarData[,] _map;

        private int _mapX;

        private int _mapY;

        private Vector2 _size;

        private int[] _x = new[] { 1, -1, 0, 0, 1, 1, -1, -1 };

        private int[] _y = new[] { 0, 0, 1, -1, 1, -1, 1, -1 };

        private int[] _g = new[] { 10, 10, 10, 10, 14, 14, 14, 14 };

        // private Mesh _rect;

        public void Init(string json)
        {
            MapData map = JsonConvert.DeserializeObject<MapData>(json);

            _size = map.size;

            int[,] mapData = map.map;
            _mapX = mapData.GetLength(0);
            _mapY = mapData.GetLength(1);

            _map = new AStarData[_mapX, _mapY];

            for (int i = 0; i < _mapX; i++)
            {
                for (int j = 0; j < _mapY; j++)
                {
                    AStarData data = new AStarData();
                    data.x = i;
                    data.y = j;
                    data.index = i * j + j;
                    data.enable = mapData[i, j] == 1;

                    _map[i, j] = data;
                }
            }
        }
        
        public List<AStarData> Search(Vector2 start, Vector2 end)
        {
            (int, int) startIndex = GetChunk(start);
            (int, int) endIndex = GetChunk(end);

            AStarData startNode = _map[startIndex.Item1, startIndex.Item2];
            AStarData endNode = _map[endIndex.Item1, endIndex.Item2];

            startNode.g = 0;
            startNode.h = GetH(startNode, endNode);
            _openList.Add(startNode);

            return FindPath(endNode);
        }

        private (int, int) GetChunk(Vector2 pos)
        {
            float halfX = _size.x * 0.5f;
            float halfY = _size.y * 0.5f;

            int x = (int)((pos.x - halfX) / _size.x);
            int y = (int)((pos.y - halfY) / _size.y);

            return (x, y);
        }

        private List<AStarData> FindPath(AStarData end)
        {
            while (_openList.Count > 0)
            {
                AStarData node = _openList.Min;
                _openList.Remove(node);

                if (node == end)
                {
                    return GetPath(node);
                }

                //添加父节点到closeList
                _closeList.Add(node);

                //遍历邻居节点
                for (int i = 0; i < _x.Length; i++)
                {
                    int neighborX = node.x + _x[i];
                    int neighborY = node.y + _y[i];

                    if (neighborX < 0 || neighborY < 0 || neighborX > _mapX - 1 || neighborY > _mapY - 1)
                    {
                        continue;
                    }

                    var neighbor = _map[neighborX, neighborY];
                    //节点不可走或者已经在closeList存在的情况下 跳过
                    if (!neighbor.enable || _closeList.Contains(neighbor))
                    {
                        continue;
                    }

                    //当前父节点G + 邻居节点权重
                    int tempG = node.g + _g[i];

                    //当前父节点到当前邻居节点开销比原邻居节点开销小的情况下
                    //openList不存在当前邻居节点情况下
                    if (tempG < neighbor.g || !_openList.Contains(neighbor))
                    {
                        //修改当前邻居节点数据
                        neighbor.g = tempG;
                        neighbor.h = GetH(neighbor, end);
                        neighbor.parent = node;

                        //添加当前邻居节点到openList
                        if (!_openList.Contains(neighbor))
                        {
                            _openList.Add(neighbor);
                        }
                    }
                }
            }

            return null;
        }

        private int GetH(AStarData start, AStarData end)
        {
            return Math.Abs(end.x - start.x) * 10 + Math.Abs(end.y - start.y) * 10;
        }

        /// <summary>
        /// 获得路径
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private List<AStarData> GetPath(AStarData node)
        {
            List<AStarData> res = new List<AStarData>() { node };

            while (node.parent != null)
            {
                res.Add(node.parent);
                node = node.parent;
            }

            res.Reverse();
            return res;
        }
        
        // ---- 绘制Gizmos -----
        
         public void ShowMapGizmos(Color color, float y)
        {
            for (int i = 0; i < _mapX; i++)
            {
                for (int j = 0; j < _mapY; j++)
                {
                    var node = _map[i, j];
                    if (node.enable)
                    {
                        DrawRect(node, color, y);
                    }
                    else
                    {
                        DrawSolidRect(node, Color.black, y);
                    }
                }
            }
        }

        public void ShowPath(List<AStarData> path, Color color, float y)
        {
            foreach (var data in path)
            {
                DrawSolidRect(data, color, y);
            }
        }

        /// <summary>
        /// 绘制矩形
        /// </summary>
        /// <param name="node"></param>
        /// <param name="color"></param>
        /// <param name="y"></param>
        private void DrawRect(AStarData node, Color color, float y)
        {
            float halfX = _size.x * 0.5f;
            float halfY = _size.y * 0.5f;
            Vector3 tl = new Vector3(node.x - halfX, y, node.y + halfY);
            Vector3 tr = new Vector3(node.x + halfX, y, node.y + halfY);
            Vector3 bl = new Vector3(node.x - halfX, y, node.y - halfY);
            Vector3 br = new Vector3(node.x + halfX, y, node.y - halfY);

            Gizmos.color = color;
            Gizmos.DrawLine(tl, tr);
            Gizmos.DrawLine(tr, br);
            Gizmos.DrawLine(br, bl);
            Gizmos.DrawLine(bl, tl);
        }

        private void DrawSolidRect(AStarData node, Color color, float y)
        {
            float halfX = _size.x * 0.5f;
            float halfY = _size.y * 0.5f;
            Vector3 tl = new Vector3(node.x - halfX, y, node.y + halfY);
            Vector3 tr = new Vector3(node.x + halfX, y, node.y + halfY);
            Vector3 bl = new Vector3(node.x - halfX, y, node.y - halfY);
            Vector3 br = new Vector3(node.x + halfX, y, node.y - halfY);

            Gizmos.color = color;
            Gizmos.DrawLine(tl, tr);
            Gizmos.DrawLine(tr, br);
            Gizmos.DrawLine(br, bl);
            Gizmos.DrawLine(bl, tl);
            Gizmos.DrawLine(tl, br);
            Gizmos.DrawLine(tr, bl);
        }

        private Mesh CreateRectangleMesh()
        {
            float halfX = _size.x * 0.5f;
            float halfY = _size.y * 0.5f;

            Vector3 tl = new Vector3(-halfX, 0, +halfY);
            Vector3 tr = new Vector3(+halfX, 0, +halfY);
            Vector3 bl = new Vector3(-halfX, 0, -halfY);
            Vector3 br = new Vector3(+halfX, 0, -halfY);

            Vector3[] vertices = new Vector3[]
            {
                tl,
                tr,
                br,
                bl
            };

            int[] triangles = new int[]
            {
                0, 1, 2,
                2, 3, 0
            };
            
            Vector3[] normals = new Vector3[]
            {
                Vector3.forward,
                Vector3.forward,
                Vector3.forward,
                Vector3.forward
            };

            var rectangleMesh = new Mesh();
            rectangleMesh.vertices = vertices;
            rectangleMesh.triangles = triangles;
            rectangleMesh.normals = normals;

            return rectangleMesh;
        }
    }
}