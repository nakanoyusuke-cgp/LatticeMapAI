using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

namespace LatticeMap
{
    // 式木で使ってますがあんまり気にしないでください。
    using Binary = Func<ParameterExpression, ParameterExpression, BinaryExpression>;
    
    // おなじみ引数デリゲート
    public delegate void MOFunctionByVector(Vector2Int coordinate);
    public delegate void MOFunctionByInt(int x, int y);

    /// <summary>
    /// 一応下のGenericMapクラスがメインですが、IConvertibleを継承していないクラスで配列を作りたい場合こちらを使用してください。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UnConvertibleGenericMap<T>
    {
        /// <summary>
        /// 2次元配列のLength
        /// </summary>
        public Vector2Int Range { get; private set; }
        
        /// <summary>
        /// 2次元配列の本体
        /// </summary>
        public T[,] Matrix { get; }
        
        /// <summary>
        /// 二次元配列初期化用デリゲート　x,yの値によって初期値を決められる
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public delegate T ValueOfInitializeMap(int x, int y);

        // コンストラクタ2種
        public UnConvertibleGenericMap(Vector2Int range, ValueOfInitializeMap function = null)
        {
            Range = range;
            Matrix = new T[range.x, range.y];
            if (function == null) return;
            MatrixOperate((x, y) => Matrix[x,y] = function(x,y));
        }
        public UnConvertibleGenericMap(T[,] matrix)
        {
            Range = new Vector2Int(matrix.GetLength(0), matrix.GetLength(1));
            Matrix = new T[Range.x, Range.y];
            Array.Copy(matrix, Matrix, matrix.Length);
        }
        
        // インデクサ2種
        public T this[int xIndex, int yIndex]
        {
            get => Matrix[xIndex, yIndex];
            set => Matrix[xIndex, yIndex] = value;
        }
        public T this[Vector2Int index]
        {
            get => Matrix[index.x, index.y];
            set => Matrix[index.x, index.y] = value;
        }
        
        // ただの2重ループ
        public void MatrixOperate(MOFunctionByInt function)
        {
            for(int x = 0; x < Range.x; x++)
                for(int y = 0; y < Range.y; y++)
                    function(x,y);
        }
        // Vector2Int使いたい人用
        public void MatrixOperate(MOFunctionByVector function) => MatrixOperate((x,y)=>function(new Vector2Int(x,y)));
        
        // 2つのマップのサイズがあってるか確認する
        protected bool RangeCheck(UnConvertibleGenericMap<T> right)
        {
            if (this.Range == right.Range) return true;
            Debug.LogError("Augments have different ranges");
            return false;
        }
    }
    
    /// <summary>
    /// タイルマップに特化した2次元配列管理クラス
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericMap<T> : UnConvertibleGenericMap<T> where T:IConvertible
    {
        // コンストラクタ2種
        public GenericMap(Vector2Int range, ValueOfInitializeMap function = null) : base(range, function){}
        public GenericMap(T[,] matrix) : base(matrix){}

        // 演算子オーバーロード
        public static implicit operator GenericMap<float>(GenericMap<T> map)=>new GenericMap<float>(map.Range, (x,y)=>(float)Convert.ToDouble(map[x,y]));
        public static implicit operator GenericMap<double>(GenericMap<T> map)=>new GenericMap<double>(map.Range, (x,y)=>Convert.ToDouble(map[x,y]));
        public static implicit operator GenericMap<decimal>(GenericMap<T> map)=>new GenericMap<decimal>(map.Range, (x,y)=>Convert.ToDecimal(map[x,y]));
        public static explicit operator GenericMap<int>(GenericMap<T> map)=>new GenericMap<int>(map.Range, (x,y)=>Convert.ToInt32(map[x,y]));
        public static explicit operator GenericMap<long>(GenericMap<T> map)=>new GenericMap<long>(map.Range, (x,y)=>Convert.ToInt64(map[x,y]));
        public static GenericMap<T> operator +(GenericMap<T> l, GenericMap<T> r) => l.RangeCheck(r) ? new GenericMap<T>(l.Range, (x,y) => Operator(Expression.Add)(l[x,y], r[x,y])) : new GenericMap<T>(l.Range);
        public static GenericMap<T> operator -(GenericMap<T> l, GenericMap<T> r) => l.RangeCheck(r) ? new GenericMap<T>(l.Range, (x,y) => Operator(Expression.Subtract)(l[x,y], r[x,y])) : new GenericMap<T>(l.Range);
        public static GenericMap<T> operator *(GenericMap<T> l, GenericMap<T> r) => l.RangeCheck(r) ? new GenericMap<T>(l.Range, (x,y) => Operator(Expression.Multiply)(l[x,y], r[x,y])) : new GenericMap<T>(l.Range);
        public static GenericMap<T> operator /(GenericMap<T> l, GenericMap<T> r) => l.RangeCheck(r) ? new GenericMap<T>(l.Range, (x,y) => Operator(Expression.Divide)(l[x,y], r[x,y])) : new GenericMap<T>(l.Range);
        public static GenericMap<T> operator %(GenericMap<T> l, GenericMap<T> r) => l.RangeCheck(r) ? new GenericMap<T>(l.Range, (x,y) => Operator(Expression.Modulo)(l[x,y], r[x,y])) : new GenericMap<T>(l.Range);
        public static GenericMap<T> operator +(GenericMap<T> l, T r) => new GenericMap<T>(l.Range, (x,y)=>Operator(Expression.Add)(l[x,y], r));
        public static GenericMap<T> operator -(GenericMap<T> l, T r) => new GenericMap<T>(l.Range, (x,y)=>Operator(Expression.Subtract)(l[x,y], r));
        public static GenericMap<T> operator *(GenericMap<T> l, T r) => new GenericMap<T>(l.Range, (x,y)=>Operator(Expression.Multiply)(l[x,y], r));
        public static GenericMap<T> operator /(GenericMap<T> l, T r) => new GenericMap<T>(l.Range, (x,y)=>Operator(Expression.Divide)(l[x,y], r));
        public static GenericMap<T> operator %(GenericMap<T> l, T r) => new GenericMap<T>(l.Range, (x,y)=>Operator(Expression.Modulo)(l[x,y], r));
        
        // 一度使用された演算子のデリゲートが格納される
        private static readonly Dictionary<Binary, Func<T, T, T>> OpDictionary = new Dictionary<Binary, Func<T, T, T>>();

        // 演算子のデリゲート辞書から値取り出し 無ければ格納
        private static Func<T, T, T> Operator(Binary op)
        {
            if(!OpDictionary.ContainsKey(op))
                OpDictionary.Add(op, CreateOperator(op));
            return OpDictionary[op];
        }
        
        // 演算子オーバーロード用の式木
        private static Func<T, T, T> CreateOperator(Binary op)
        {
            var l = Expression.Parameter(typeof(T), "l");
            var r = Expression.Parameter(typeof(T), "r");
            var expression = Expression.Lambda<Func<T, T, T>>(op(l, r), l, r);
            return expression.Compile();
        }
    }
}
