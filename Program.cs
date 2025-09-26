using Python.Runtime; // Python.NETが必要です。NuGetでインストールしてください。
using System.Collections.Generic;
using System;
using System.Dynamic;
using System.IO;
namespace onnseiproject
{

    class Program
    {
        private static dynamic voicevox_core;
        private static dynamic sf;
        private static dynamic sd;
        private static dynamic np;
        private static dynamic sr;
        private static dynamic Path;
        private static dynamic? core_instance;
        private static readonly HashSet<int> loadedModelIds = new HashSet<int>();
        private static readonly object modelLock = new object();
        private static dynamic spid;
        private static void PreloadModels(params int[] speakerIds)
        {
            using (Py.GIL())
            {
                if (core_instance == null)
                {
                    core_instance = voicevox_core.VoicevoxCore(
                        open_jtalk_dict_dir: "/Users/M2Seito051/Downloads/onnseiproject-main/open_jtalk_dic_utf_8-1.11",
                        acceleration_mode: "AUTO"
                    );
                }

                foreach (var id in speakerIds)
                {
                    if (!loadedModelIds.Contains(id))
                    {
                        core_instance.load_model(id);
                        loadedModelIds.Add(id);
                        Console.WriteLine($"モデルID {id} をロードしました");
                    }
                }
            }
        }
        static void Main(string[] args)
        {
            Runtime.PythonDLL = "/Library/Frameworks/Python.framework/Versions/3.9/lib/libpython3.9.dylib";
            PythonEngine.Initialize();
            using (Py.GIL())
            {
                sd = Py.Import("sounddevice");
                sf = Py.Import("soundfile");
                sr = Py.Import("speech_recognition");
                voicevox_core = Py.Import("voicevox_core");
                Path = Py.Import("pathlib");
                np = Py.Import("numpy");

                int dvid;
                Console.WriteLine(sd.query_devices());
                Console.WriteLine("再生デバイスを選択してください。選択がなかった場合はデフォルトのデバイスになります。");
                try
                {
                    dvid = int.Parse(Console.ReadLine());

                }
                catch
                {
                    Console.WriteLine("選択してください");
                    return;
                }
                Console.WriteLine("四国めたん（ノーマル）：2四国めたん（あまあま）：0\n四国めたん（ツンツン）：6\n四国めたん（セクシー）：4\n四国めたん（ささやき）：36\n四国めたん（ヒソヒソ）：37\nずんだもん（ノーマル）：3\nずんだもん（あまあま）：1\nずんだもん（ツンツン）：7\nずんだもん（セクシー）：5\nずんだもん（ささやき）：22\nずんだもん（ヒソヒソ）：38\n春日部つむぎ（ノーマル）：8\n雨晴はう（ノーマル）：10\n波音リツ（ノーマル）：9\n玄野武宏（ノーマル）：11\n玄野武宏（喜び）：39\n玄野武宏（ツンギレ）：40\n玄野武宏（悲しみ）：41\n白上虎太郎（ふつう）：12\n白上虎太郎（わーい）：32\n白上虎太郎（びくびく）：33\n白上虎太郎（おこ）：34\n白上虎太郎（びえーん）：35\n青山龍星（ノーマル）：13\n冥鳴ひまり（ノーマル）：14\n九州そら（ノーマル）：16\n九州そら（あまあま）：15\n九州そら（ツンツン）：18\n九州そら（セクシー）：17\n九州そら（ささやき）：19\nもち子さん（ノーマル）：20\n剣崎雌雄（ノーマル）：21\nWhiteCUL（ノーマル）：23\nWhiteCUL（たのしい）：24\nWhiteCUL（かなしい）：25\nWhiteCUL（びえーん）：26\n後鬼（人間ver.）：27\n後鬼（ぬいぐるみver.）：28\nNo.7（ノーマル）：29\nNo.7（アナウンス）：30\nNo.7（読み聞かせ）：31\nちび式じい（ノーマル）：42\n櫻歌ミコ（ノーマル）：43\n櫻歌ミコ（第二形態）：44\n櫻歌ミコ（ロリ）：45\n小夜 / SAYO（ノーマル）：46\nナースロボ＿タイプＴ（ノーマル）：47\nナースロボ＿タイプＴ（楽々）：48\nナースロボ＿タイプＴ（恐怖）：49\nナースロボ＿タイプＴ（内緒話）：50\nモデルIDを指定してください。指定がなかった場合は1になります。");
                try
                {
                    spid = int.Parse(Console.ReadLine());

                }
                catch
                {
                    spid = 1;
                    Console.WriteLine("指定がなかったので1になります。");
                }
                PreloadModels(spid);
                while (true)
                {
                    dynamic text = null;
                    text = Listentotext();
                    if (text == null)
                    {
                        text = "";
                    }
                    Console.WriteLine(text);
                    dynamic waveBytes = Makevoice(text, spid);
                    dynamic audio_data = np.frombuffer(waveBytes, dtype: np.int16);
                    sd.play(audio_data, samplerate: 24000, device: dvid);
                    Console.WriteLine("再生完了");
                }
            }
        }
        static dynamic Makevoice(dynamic text, int id)
        {
            using (Py.GIL())
            {
                if (core_instance != null)
                {
                    return core_instance.tts(text, id);
                }
                else
                {
                    core_instance = voicevox_core.VoicevoxCore(
                        open_jtalk_dict_dir: "/Users/hakumai/Documents/develop/csharp/onnseiproject/open_jtalk_dic_utf_8-1.11",
                        acceleration_mode: "AUTO"
                    );
                    core_instance.load_model(id);
                    return core_instance.tts(text, id);
                }

            }
        }
        static dynamic Listentotext()
        {

            dynamic recog = sr.Recognizer();
            recog.energy_threshold = 300;  // 音声検出の閾値を調整
            recog.dynamic_energy_threshold = false;  // 動的閾値を無効化して処理を高速化
            recog.pause_threshold = 0.5; // 発話の間隔を短く設定
            recog.non_speaking_duration = 0.3;  // 無音判定の時間を短縮

            using (dynamic source = sr.Microphone().__enter__())  // Pythonのwith文に相当
            {
                source.sample_rate = 16000;  // サンプルレートを下げて処理を軽く
                source.chunk_size = 1024;    // チャンクサイズを調整

                Console.WriteLine("話してください...");
                try
                {
                    dynamic audio = recog.listen(source, timeout: 5, phrase_time_limit: 10);
                    return recog.recognize_google(audio, language: "ja-JP");
                }
                catch
                {
                    return "";
                }

            }
        }
    }

}