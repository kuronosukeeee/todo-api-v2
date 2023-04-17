// 以下のインポート文はコード内で使用されないものも含まれるが、ビルドまたは実行時にASP.NET Coreが暗黙的に使用するため、すべて必要となる
//① Httpリクエスト処理パイプライン（リクエスト→処理→レンスポンスの一連の流れ）を構築するためのクラス
using Microsoft.AspNetCore.Builder;
// ②ホスト環境を構築するためのクラス
using Microsoft.AspNetCore.Hosting;
// ③EF Coreの機能を提供するクラス
using Microsoft.EntityFrameworkCore;
// ④DIコンテナに関連するクラス
using Microsoft.Extensions.DependencyInjection;
// ⑤ホスト環境に関する情報を提供するクラス
using Microsoft.Extensions.Hosting;

// Models名前空間のインポート（データベースコンテキスの追加でTodoContextを使用）
using TodoApi.Models;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// builderオブジェクトの生成（アプリの設定やアプリの（コア）機能（＝サービス）の登録で使用）
var builder = WebApplication.CreateBuilder(args);

//CORSポリシーの設定（特定の（自身とは異なる）オリジン（プロトコル＋ドメイン＋ポート）からの接続を許可する設定（通常は拒否される））の追加（ほぼテンプレ）
builder.Services.AddCors(options =>
{
    //ここでMyAllowSpecificOrigins（次の宣言が必要「var ポリシー名 = "_myAllowSpecificOrigins";）という名前で管理されるCROSポリシーを定義
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                          
                      });
});

// MVCモデルのControllerを追加
builder.Services.AddControllers();

// appsettings.jsonからデータベース接続文字列を取得し変数に格納
var connectionString = builder.Configuration.GetConnectionString("TodoContext");
// データベースコンテキストの追加（PostgreSQLを使用）
builder.Services.AddDbContext<TodoContext>(options => options.UseNpgsql(connectionString));

// アプリケーションインスタンスの生成
var app = builder.Build();

// 開発環境の場合はSwagger(SwaggerUI)を使用するための設定（自動生成）
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// Httpでアクセスされた場合はHttpsにリダイレクトする設定（自動生成）
// app.UseHttpsRedirection();

//ここでCORSミドルウェア（OSとアプリケーションを繋ぐもの（WEBサーバーとかデータベースとか））を実行（実行順序が大事：UseRoutingの後でUseAuthorizationの前）
app.UseCors(MyAllowSpecificOrigins);

// 認証・認可機能を有効にする設定（自動生成）
// app.UseAuthorization();

// ControllerをHttpリクエストにマッピング
app.MapControllers();

// アプリの実行
app.Run();
