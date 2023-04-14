// EF Coreでデータベースとマッピングされるクラス（以下の3つの制約がある）
//    ①クラス名はテーブル名と同名
//    ②プロパティはテーブル列と同名
//    ③主キーは「Id」
//    （①、②に関してはEF Coreのマイグレーション（データベースのスキーマ（構造）を変更するための手段）を利用することで正しいスキーマが使用されることが保証される）

//C#の基本的な機能を提供する名前空間をインポート
using System;
// null許容参照型を無効にするディレクティブ（つまり、デフォルト値を非null許容型にする）
// 不用意にnullが入るとクラッシュするリスクがあるためそれを回避するC#の機能
// 参照型にのみ機能する点に注意（プリミティブ型は効果の適用範囲外）
#nullable disable
// 名前空間を指定することでコードを構造化
namespace TodoApi.Models
{
  public class TodoItem
  {
    // {get; set;}はアクセサ
    // EF Coreの機能によって自動的に一意のIdが生成される（Idプロパティが整数型の場合に限る）
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    // DueDateは必ず設定されるべきなので非null許容型
    public DateTime DueDate { get; set; }
    // CompletedDateは完了時に設定されるべきなのでnull許容型
    public DateTime? CompletedDate { get; set; }
    public bool IsCompleted { get; set; }
  }
}