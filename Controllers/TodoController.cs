// ASP.NET Core MVCの基本機能を提供するクラス
using Microsoft.AspNetCore.Mvc;
// EF Coreの機能を提供するクラス
using Microsoft.EntityFrameworkCore;
// LINQの機能を提供するクラス
using System.Linq;
// Models名前空間のインポート（TodoItemとTodoContextクラスを使用）
using TodoApi.Models;

namespace TodoApi.Controllers
{
  // APIコントローラであることを示す（apiに不要なMVC機能(viewなど)を無効化）
  [ApiController]
  // コントローラーのルーティングの定義（"[controller]"はコントローラー名から「Controller」を除いたものに置き換わる（つまりこの場合は"api/Todo"）
  [Route("api/[controller]")]
  // ControllerBaseクラスを継承（コントローラーの基本機能を継承）
  public class TodoController : ControllerBase
  {

    // TodoContextクラスのインスタンスを保持するプライベートフィールドの定義（データベースへのアクセスを伴うアクションメソッド実行時に使用される）
    // 慣例的に、可読性の観点からプライベートフィールドの名前は_から始める
    private readonly TodoContext _context;
    // コンストラクタでTodoContextを受け取り、フィールドに代入（DIによってTodoContextクラスのインスタンスが提供される）
    public TodoController(TodoContext context)
    {
      _context = context;
    }

    // 以下はアクションメソッド（apiのコア機能、一つ以上のアクションメソッドから構成されるものがコントローラー）

    // タスクの全件取得
    // Http GETリクエストを処理するアクションメソッドであることを示す（エンドポイント： api/Todo）
    [HttpGet]
    // 非同期実行できるように実装（非同期実行の条件は以下の3つ）
    //    ①async修飾詞をつける（awaitを利用するための宣言）
    //    ②戻り値はTask型（スレッドプール（オーバーヘッドを減らすためにスレッドを再利用する仕組み）を使用するためのクラス）
    //    ③非同期実行したい処理にawait演算子を付与（await演算子を付与した部分以外は同期的に順次実行される点に注意）
    //    （非同期処理のメソッド名は可読性の観点から〜Asyncとすることが一般的）    
    // アクションメソッドの戻り値はActionResult型（httpステータスコードや返すデータなどを含む）もしくはその派生型＋ジェネリック（汎用的なクラスやメソッドを特定の型に対応付ける機能）
    // データの取得処理は、反復処理（イテレーション）やフィルタリング、ソートなどが簡単にできるようにコレクションとして取得することが一般的
    // IEnumerable<T>は上記の機能（コレクションに対する反復処理）を可能にするためのインターフェースであり、これを戻り値の型として指定する事で以下の2つのメリットが得られる
    //    ①柔軟性：戻り値がIEnumerable<T>であれば、具体的なコレクション型（リストや配列など）に依存しないため、柔軟なコードが書ける（将来的にコレクションの具体的な実装が変わっても、インターフェースが同じであればコードを修正する必要がない（全てのコレクションはIEnumerable<T>インターフェースを実装している））
    //    ②可読性：戻り値がIEnumerable<T>であることで、このメソッドがコレクションを返すことが明らかになる
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
    {
      // EF Coreはテーブルに対応するModelを使用して操作する（この場合はTodoItem）
      // 記述方法は「Dbcontext.Model.EF Coreのメソッド」
      // ToListAsyncは全てのTodoItemを（非同期的に）リストに変換するEF Coreのメソッド
      return await _context.TodoItem.ToListAsync();
    }

    // 未完了タスクの取得
    // incompleteパスを追加（エンドポイント：api/Todo/incomplete）
    [HttpGet("incomplete")]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetIncompleteTodoItem()
    {
      // EF Core＋LINQ（whereによるフィルタリング）を使ったクエリの発行形式は以下の通り
      // 「Dbcontext.Model.LINQ（フィルタリング、ソート、グルーピング）.EF Coreのメソッド」
      // LINQのwhereメソッドは、コレクション内の各要素を取り出し、ラムダ式で指定された処理を適用する（todoItemはその際にコレクションから取り出した要素を一時的に格納するための変数（TodoItemモデルのIsCompletedプロパティを参照））
      return await _context.TodoItem.Where(todoItem => !todoItem.IsCompleted).ToListAsync();
    }

    // 完了済タスクの取得（エンドポイント：api/Todo/completed）
    // 仕組みは未完了タスクの取得と同じ
    [HttpGet("completed")]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetCompletedTodoItem()
    {
      return await _context.TodoItem.Where(todoItem => todoItem.IsCompleted).ToListAsync();
    }

    // タスクの追加（エンドポイント：api/Todo）
    [HttpPost]
    public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
    {
      // 例外処理は各条件が独立しているため（一度に1つの条件しか満たさないため）if文を連続して使用することが一般的
      // タスクの内容が100文字以内であるかどうかを判定するためにStringクラスのLengthプロパティを使用する
      if (todoItem.Description.Length > 100)
      {
        // BadRequestはControllerBaseクラスが提供するヘルパーメソッド（400 BadRequestを返し、クライアントが不正なリクエストを送信したことを示す）
        return BadRequest();
      }
      // 期日が過去日の場合はBadRequestを返す
      // DateTimeクラスのUtcNowプロパティは現在の日時を表すDateTimeオブジェクトをUTC(協定世界時)で取得する
      if (todoItem.DueDate < DateTime.UtcNow)
      {
        return BadRequest("期日に過去の日付が設定されています");
      }
     //期日がUTCではない場合はCompletedDateをUTCに変換する
      if (todoItem.DueDate.Kind != DateTimeKind.Utc)
      {
        todoItem.DueDate = todoItem.DueDate.ToUniversalTime();
      }
      //タスクが完了している＋完了日が設定されている＋完了日がUTCではない場合はCompletedDateをUTCに変換する
      if (todoItem.IsCompleted && todoItem.CompletedDate.HasValue && todoItem.CompletedDate.Value.Kind != DateTimeKind.Utc)
      {
        todoItem.CompletedDate = todoItem.CompletedDate.Value.ToUniversalTime();
      }
      // Addは引数で受け取ったtodoItemインスタンスを新しいレコードとしてTodoItemテーブルに挿入するEF Coreのメソッド
       _context.TodoItem.Add(todoItem);
      // SaveChangesAsyncはAddメソッドで追加したデータをデータベースに反映（保存）するEF Coreのメソッド
      await _context.SaveChangesAsync();
      // CreatedAtActionはControllerBaseが提供するヘルパーメソッド（Httpステータスコード201（Created）を返す）
      // CreatedAtActionは引数を3つ取って、生成されたレコードへのURIを返すことが一般的（引数は順に、URIを生成するための取得に関するアクションメソッド名,追加したレコードのid,追加したレコードの内容を含むモデルのインスタンス）
      // nameof演算子はクラス名やメソッド名などを単なる文字列情報に変換する
      // new{メンバー名 = 初期値}は匿名型と呼ばれるC#の機能（匿名オブジェクト（名前を持たないクラスのインスタンス）を生成し、読み取り専用のプロパティを持たせることができる）
      return CreatedAtAction(nameof(GetTodoItems), new { id = todoItem.Id }, todoItem);
    }

    // タスクの編集（更新）（エンドポイント：api/Todo/{id}）
    // EF Coreを使用している場合は具体的な更新処理は不要である点に注意する（以下UpdateTodoItemメソッドのおおまかな挙動）
    //    前提：通常EF Coreはエンティティの変更を自動追跡するが、ここでは自動追跡機能は使わず、受け取ったタスクが更新済であることを手動でEF Coreに通知する（クライアントサイドから送信されるタスクは編集済であるとみなす（つまり、受け取ったタスクとデータベースから取得したタスクの比較処理を行わない））
    //    ①クライアントから送信された更新済のtodoItemがUpdateTodoItemメソッドの引数として受け取られる
    //    ②「_context.Entry(todoItem).State = EntityState.Modified」によって受け取ったtodoItemが変更された状態であることをEF Coreに通知
    //    ③SaveChangesAsyncメソッドを呼び出して、EF Coreに変更追跡されているエンティティ（todoItem）に対応するデータベースのレコードを更新させる
    // {}はAsp.NetCoreの（ルーティング機能における）ルートパラメータ
    // リクエストのURIから（対応する箇所を）{}で括られた名前に抽出し、アクションメソッドに渡す
    // モデルクラスのパラメータとは無関係でルートパラメータとアクションメソッドの引数名は同じである必要がある
    [HttpPut("{id}")]
    // IActionResultはActionResultの派生型であり、より詳細なHttpステータスコードやレスポンスデータを返すことができる
    // タスクの編集機能では様々なHttpステータスコードを返す必要があるため、戻り値の型はTask<IActionResult>型（Iはインターフェースを表す）とする
    public async Task<IActionResult> UpdateTodoItem(long id, TodoItem todoItem)
    {
      if (id != todoItem.Id)
      {
        return BadRequest();
      }
      if (todoItem.Description.Length > 100)
      {
        return BadRequest();
      }
    //期日がUTCではない場合はCompletedDateをUTCに変換する
    if (todoItem.DueDate.Kind != DateTimeKind.Utc)
    {
        todoItem.DueDate = todoItem.DueDate.ToUniversalTime();
    }
    // タスクが完了済でかつ、タスク完了日時が設定されていないものには現在の日時を設定する
    if (todoItem.IsCompleted && todoItem.CompletedDate == null)
    {
        // 現在時刻を取得する処理はタイムゾーンを考慮しない絶対的な時刻であるUTCを使用し、日本時間(JST)で時刻を表示する場合はクライアントサイドで実装（UTC→JSTへ変換）することが一般的
        todoItem.CompletedDate = DateTime.UtcNow;
      }
      // Entry(todoItem)はコンテキスト内におけるエンティティの追跡情報を取得するEF Coreのメソッド
      // Stateはエンティティの現在の状態を取得または設定するEntryメソッドのプロパティ（状態にはUnchanged,Added,Deleted,Detachedなどがある）
      // EntityStateはEF Coreが提供するエンティティの状態を示す列挙型（Modifiedはエンティティが変更されたことを示す）
      _context.Entry(todoItem).State = EntityState.Modified;
      // 例外処理
      // 前提：ASP.NET Coreでは、例外が発生すると組み込みの例外処理機能がその例外をキャッチして適切なHttpステータスコードやエラーメッセージを返すことができる
      try
      {
        // 更新されているエンティティ（todoItem）に対応するデータベースのレコードを非同期的に更新する
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        // ここでは、DbUpdateConcurrencyException（データベース更新操作中に同時実行制御（複数のユーザーが同時にデータベースの同じデータを操作した際に、データの生合成を保つための仕組み）に関連する問題が発生した場合にスローされる例外）の発生原因の内、「idが一致しなかった場合」のみ、NotFoundを返すようにしておくことが一般的なので個別に例外処理を記述する（データベースの更新操作において、エンティティが存在しない、または既に削除されているという事象はよくあるため）
        if (!_context.TodoItem.Any(e => e.Id == id))
        {
          // NotFoundはControllerBaseクラスが提供するヘルパーメソッド（404 NotFoundを返し、リクエストされたリソースが見つからなかったことを示す）
          return NotFound();
        }
        // それ以外の場合は、throw文を用いてフレームワークに例外を再スローし、適切なHttpステータスコードやエラーメッセージを返すようにする
        else
        {
          throw;
        }
      }
      // NoContentはControllerBaseクラスが提供するヘルパーメソッド（204 NoContentを返し、リクエストが正常に処理され、返すべきコンテンツがないことを示す）
      return NoContent();
    }

    // タスクの削除（エンドポイント：api/Todo/{id}）
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodoItem(long id)
    {
      // 引数で受け取ったidをデータベース内で検索し、オブジェクト（todoItem）に代入する
      // varは型推論（何でもありのvariant型ではない点に注意、代入された値からコンパイラが自動的に型を推論する）
      // FirstOrDefaultAsync（ラムダ式を用いた条件式）はコレクションとして取得したデータベースの全てのレコードに対して、引数で受け取った条件式を当てはめ、最初に一致した要素を返すEF Coreのメソッド（コレクション（データベース）内に条件式に当てはまるものがなければ参照型であればnullを、プリミティブ型であればそのデフォルト値を返す）
      var todoItem = await _context.TodoItem.FirstOrDefaultAsync(i => i.Id == id);
      // データベース内に削除対象となるタスクが存在しない場合はNotFoundを返す
      if (todoItem == null)
      {
        return NotFound();
      }
      // Removeは引数で渡したエンティティをデータベースから削除する指示を送るEF Coreのメソッド（この時点では削除されない点に注意、SaveChangesAsyncを呼び出すことで削除操作が行われる）
      _context.TodoItem.Remove(todoItem);
      // ここで実際に削除処理が（非同期的に）行われる
      await _context.SaveChangesAsync();
      return NoContent();
    }
  }
}
