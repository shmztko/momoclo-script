
namespace ChangeWallpaper
{
    using System;
    using System.IO;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.Win32;

    class Change
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("ChangeWallpaper command start.");
            try
            {
                if (args.Length < 1)
                {
                    throw new ArgumentException("画像ファイルへのパスをコマンドラインの引数に指定してください。");
                }

                string imagePath = args[0];
                new WallpaperChanger().Change(imagePath, WallpaperStyle.FitHeight);

                Environment.Exit(0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Environment.Exit(1);
            }
            finally
            {
                Console.WriteLine("ChangeWallpaper command finished.");
            }

        }
    }

    class WallpaperChanger
    {

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIini);

        /// <summary>
        /// 指定されたパスの画像を壁紙に設定します。
        /// </summary>
        /// <param name="filePath">壁紙として設定する画像ファイルのパス</praram>
        /// <param name="style">壁紙の設定方法</param>
        public void Change(string filePath, WallpaperStyle style)
        {
            Console.WriteLine("Changing wallpaper -> " + filePath);

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("ファイル名が空", "file");
            }

            if (style == null)
            {
                throw new ArgumentNullException("style");
            }
             
            // HKEY_CURRENT_USER\Control Panel\Desktopを開く
            using (var regkeyDesktop = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
            {
                if (regkeyDesktop == null)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                // TileWallpaperとWallpaperStyleを設定
                regkeyDesktop.SetValue("TileWallpaper", style.IsTiled());
                regkeyDesktop.SetValue("WallpaperStyle", style.GetStyle());

                // 「並べて表示」、「拡大して表示」などの原点も変えたい場合は、この値を0以外に変更する
                regkeyDesktop.SetValue("WallpaperOriginX", "0");
                regkeyDesktop.SetValue("WallpaperOriginY", "0");

                // Wallpaperの値をセットすることでも壁紙を設定できるが、
                // SystemParametersInfoを呼び出さないと、壁紙を設定しても即座には反映されない
                //regkeyDesktop.SetValue("Wallpaper", file);
            }

            SetImage(filePath);

            Console.WriteLine("Wallpaper changed");
        }

        /// <summary>
        /// 壁紙を剥がします。
        /// </summary>
        public void Clear()
        {
            // ファイル名に"\0"を指定すると、
            // 壁紙をはがす(背景色のみの状態にする)ことができる
            SetImage("\0");
        }

        /// <summary>
        /// 壁紙用画像を設定します。
        /// </summary>
        /// <param name="imageFilePath">画像ファイルへのパス</param>
        private void SetImage(string imageFilePath)
        {
            const int SPI_SETDESKWALLPAPER = 0x0014; // デスクトップの壁紙を設定
            const int SPIF_UPDATEINIFILE = 0x0001; // 設定を更新する
            const int SPIF_SENDWININICHANGE = 0x0002; // 設定の更新を全てのアプリケーションに通知(WM_SETTIMGCHANGE)する

            var flags = (Environment.OSVersion.Platform == PlatformID.Win32NT)
              ? SPIF_SENDWININICHANGE
              : SPIF_SENDWININICHANGE | SPIF_UPDATEINIFILE;

            // 壁紙となるファイルを設定する
            // なお、XP以前の場合はBMP形式のみが指定可能、Vista/7では加えてJPEG形式も指定可能
            if (!SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imageFilePath, flags))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }


    /// <summary>
    /// 壁紙の表示スタイルを示すクラス
    /// </summary>
    class WallpaperStyle
    {
        /// <summary>
        /// 中央に表示
        /// </summary>
        public static readonly WallpaperStyle Center = new WallpaperStyle(0, 0);

        /// <summary>
        /// 並べて表示
        /// </summary>
        public static readonly WallpaperStyle Tile = new WallpaperStyle(0, 1);

        /// <summary>
        /// 拡大して表示 (画面に合わせて伸縮)
        /// </summary>
        public static readonly WallpaperStyle Stretch = new WallpaperStyle(2, 0);

        /// <summary>
        /// リサイズして表示 (ページ縦幅に合わせる)
        /// </summary>
        /// <remarks>Windows 7以降のみ</remarks>
        public static readonly WallpaperStyle FitHeight = new WallpaperStyle(6, 0);

        /// <summary>
        /// リサイズして全体に表示 (ページ横幅に合わせる)
        /// </summary>
        /// <remarks>Windows 7以降のみ</remarks>
        public static readonly WallpaperStyle FitWidth = new WallpaperStyle(10, 0);

        /// <summary>
        /// スタイルの種別を示すコード
        /// </summary>
        private readonly int _Style;

        /// <summary>
        /// タイル状に配置するかどうかのコード
        /// </summary>
        private readonly int _IsTiled;

        /// <summary>
        /// このクラスがインスタン化されるときに呼び出されます。
        /// </summary>
        /// <param name="style">スタイル種別を示すコード</param>
        /// <param name="isTiled">タイル状に配置するかどうかのコード</param>
        private WallpaperStyle(int style, int isTiled)
        {
            _Style = style;
            _IsTiled = isTiled;
        }

        /// <summary>
        /// タイル状に配置するかどうかのコードを取得します。
        /// </summary>
        /// <returns>タイル状に配置するかどうかのコード</returns>
        public String IsTiled()
        {
            return _IsTiled.ToString();
        }

        /// <summary>
        /// スタイルの種別を示すコードを取得します。
        /// </summary>
        /// <returns>スタイルを示すコード</returns>
        public String GetStyle()
        {
            return _Style.ToString();
        }

    }

}
