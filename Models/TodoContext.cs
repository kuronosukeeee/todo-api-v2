// EntityFrameworkCore(EF Core)はRDBとModelクラスのO/Rマッパー（テーブル＝モデル,列＝プロパティとして自動的に割り当てるためのもの、以下の3つの制約がある）
//    ①DbContextクラスを継承している
//    ②DbContextOptions<Context>型の引数を受け取るコンストラクタを持つ
//    ③DbSet<Model>型のpublicプロパティを持つ（モデルクラスのコレクション、モデルクラスへのアクセサ（＝TodoItemテーブルへのエントリポイント）として機能）

// EF Coreの機能を提供するクラス
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Models {
  // :DbContextはC#におけるクラスの継承
  public class TodoContext : DbContext {
    // :baseはC#における基底クラスコンストラクタの呼び出し
    public TodoContext(DbContextOptions<TodoContext> options) : base(options){}
    // ここはプロパティ名をモデルクラスの複数形にしてコレクションを明示した方がいいという考えもあった
    // とりあえずは参考書籍「速習ASP.NET Core MVC編」の指示に従ってモデルクラスと同名にする
    public DbSet<TodoItem>? TodoItem {get; set;}
    }
  }
