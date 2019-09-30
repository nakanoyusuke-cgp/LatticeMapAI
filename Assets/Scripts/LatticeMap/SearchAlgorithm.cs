using System;
using System.Collections.Generic;
using UnityEngine;

namespace LatticeMap
{
    /// <summary>
    /// 1ステップで到達可能な相対座標を示す。
    /// </summary>
    /// <param name="basePosition"></param>
    public delegate IEnumerable<Vector2Int> NextPoint(Vector2Int basePosition);

    /// <summary>
    /// 探索系メソッドの詰め合わせ。 mapRange省略verはDefaultRangeの値を必ず初期化してから使用すること
    /// </summary>
    public static class SearchAlgorithm
    {
        /// <summary>
        /// 探索用関数等で特にマップ範囲を指定しなかった場合この値が適用される。
        /// むやみに変更しないように
        /// </summary>
        public static Vector2Int DefaultRange = new Vector2Int();

        /// <summary>
        /// DistanceMap専用の構造体
        /// </summary>
        private struct SearchAgent
        {
            public Vector2Int position;
            public int distance;
        }

        /// <summary>
        /// DistanceMap専用のデリゲート
        /// 距離->行列のセルの値に変換する関数
        /// </summary>
        /// <param name="distance">距離、基準座標を0とする</param>
        public delegate T DistanceMapFunction<T>(int distance);
        
        /// <summary>
        /// 幅優先探索により、ある座標からの最短迂回距離を算出、距離に応じて任意の行列を生成する
        /// </summary>
        /// <param name="mapRange">マップ行列の範囲</param>
        /// <param name="basePosition">距離0となる基準の座標</param>
        /// <param name="nextPoint">ある座標から1ステップで移動可能な相対座標を示す</param>
        /// <param name="function">算出された距離に応じた値で行列を生成するための関数</param>
        public static GenericMap<T> DistanceMap<T>(Vector2Int mapRange, Vector2Int basePosition, NextPoint nextPoint, DistanceMapFunction<T> function) where T : IConvertible
        {
            var searchMatrix = new GenericMap<int>(mapRange, (x, y) => -1);
            Queue<SearchAgent> searchAgent = new Queue<SearchAgent>(){};
            searchAgent.Enqueue(new SearchAgent() { position = basePosition, distance = 0 });
            searchMatrix[basePosition] = 0;
            while (0 < searchAgent.Count)
            {
                SearchAgent current = searchAgent.Dequeue();
                foreach (var vector in nextPoint(current.position))
                    if (searchMatrix.WithinMapRange(vector))
                        if (searchMatrix[vector] == -1)
                            searchAgent.Enqueue(new SearchAgent(){position = vector, distance = searchMatrix[vector] = current.distance + 1});
            }
            return new GenericMap<T>(mapRange, (x,y)=>function(searchMatrix[x,y]));
        }
        
        /// <summary>
        /// 幅優先探索により、ある座標からの最短迂回距離を算出、距離に応じて任意の行列を生成する mapRange省略ver
        /// </summary>
        /// <param name="basePosition">距離0となる基準の座標</param>
        /// <param name="nextPoint">ある座標から1ステップで移動可能な相対座標を示す</param>
        /// <param name="function">算出された距離に応じた値で行列を生成するための関数</param>
        public static GenericMap<T> DistanceMap<T>(Vector2Int basePosition, NextPoint nextPoint, DistanceMapFunction<T> function) where T : IConvertible =>
            DistanceMap(DefaultRange, basePosition, nextPoint, function);
        
        /// <summary>
        /// judgePositionがマップの範囲内であればtrueを返す。
        /// </summary>
        /// <param name="genericMap">範囲(Range)をとるマップ</param>
        /// <param name="judgePosition">次判定する座標</param>
        /// <returns>trueなら範囲内、falseならOutOfRange</returns>
        public static bool WithinMapRange<T>(this GenericMap<T> genericMap, Vector2Int judgePosition) where T:IConvertible =>
            (judgePosition.x >= 0) && (judgePosition.y >= 0) && (judgePosition.x < genericMap.Range.x) &&
            (judgePosition.y < genericMap.Range.y);

        /// <summary>
        /// 経路探索 到達不可能or同じ座標なら空のリストを返す。
        /// </summary>
        /// <param name="range">マップの範囲</param>
        /// <param name="basePosition">経路の始点</param>
        /// <param name="destination">経路の終点</param>
        /// <param name="nextPoint">現座標から1ステップで移動可能な座標</param>
        /// <returns>経路の相対座標のリスト</returns>
        public static List<Vector2Int> PathFind(Vector2Int range, Vector2Int basePosition, Vector2Int destination, NextPoint nextPoint)
        {
            var searchMap = DistanceMap(range, destination, nextPoint, distance => distance);
            if(searchMap[destination] == -1) return new List<Vector2Int>();
            var result = new List<Vector2Int>();
            var currentPosition = basePosition;
            var step = searchMap[basePosition];
            var limit = step + 1;
            while (currentPosition != destination && limit > 0)
            {
                var nextBuffer = new Vector2Int();
                foreach (var next in nextPoint(currentPosition))
                    if (searchMap[next] < step)
                        step = searchMap[nextBuffer = next];
                result.Add(nextBuffer - currentPosition);
                currentPosition = nextBuffer;
                limit--;
            }
            if (limit == 0) Debug.LogError("The limiter has been activated!!");
            return result;
        }
        
        /// <summary>
        /// 経路探索 到達不可能or同じ座標なら空のリストを返す mapRange省略ver
        /// </summary>
        /// <param name="basePosition">経路の始点</param>
        /// <param name="destination">経路の終点</param>
        /// <param name="nextPoint">現座標から1ステップで移動可能な座標</param>
        /// <returns>経路の相対座標のリスト</returns>
        public static List<Vector2Int> PathFind(Vector2Int basePosition, Vector2Int destination, NextPoint nextPoint) =>
            PathFind(DefaultRange, basePosition, destination, nextPoint);

        /// <summary>
        /// 2座標間の迂回距離(到達に必要なステップ数) 到達不可能なら-1を返す
        /// </summary>
        /// <param name="range">2次元配列の範囲</param>
        /// <param name="basePosition">始点</param>
        /// <param name="destination">終点</param>
        /// <param name="nextPoint">現座標から1ステップで移動可能な座標</param>
        /// <returns></returns>
        public static int Distance(Vector2Int range, Vector2Int basePosition, Vector2Int destination, NextPoint nextPoint) =>
            DistanceMap(range, destination, nextPoint, distance => distance)[basePosition];
        
        /// <summary>
        /// 2座標間の迂回距離(到達に必要なステップ数) 到達不可能なら-1を返す mapRange省略ver
        /// </summary>
        /// <param name="basePosition">始点</param>
        /// <param name="destination">終点</param>
        /// <param name="nextPoint">現座標から1ステップで移動可能な座標</param>
        /// <returns></returns>
        public static int Distance(Vector2Int basePosition, Vector2Int destination, NextPoint nextPoint)=>
            DistanceMap(destination, nextPoint, distance => distance)[basePosition];
    }
}
