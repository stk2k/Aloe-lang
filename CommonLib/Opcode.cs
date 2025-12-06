using System;

namespace Aloe.CommonLib
{
    public enum Opcode : byte
    {
        Nop = 0,

        // スタック / 定数
        PushConst = 1,

        // 算術
        Add = 2,
        Sub = 3,
        Mul = 4,
        Div = 5,

        // I/O
        Print = 6,

        // 制御フロー
        Jump = 7,         // +int32 (相対オフセット)
        JumpIfFalse = 8,  // +int32 (相対オフセット)

        // 関数呼び出し
        Call = 9,         // +int32 (関数インデックス)
        Ret = 10,

        // 終了
        Halt = 11,

        // スタックユーティリティ
        Pop = 12,
        Dup = 13,

        // ローカル変数アクセス
        LoadLocal = 20,   // +int32 (ローカルインデックス)
        StoreLocal = 21,  // +int32 (ローカルインデックス)

        // 比較演算（結果は bool）
        CmpEq = 30,
        CmpNe = 31,
        CmpLt = 32,
        CmpLe = 33,
        CmpGt = 34,
        CmpGe = 35,

        // 論理演算（bool 専用）
        LogicNot = 40,
        LogicAnd = 41,
        LogicOr = 42,
    }
}
