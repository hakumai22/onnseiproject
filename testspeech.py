import sys
import speech_recognition as sr

def main():
    # マイクから音声を認識するためのRecognizerインスタンスを作成
    recognizer = sr.Recognizer()
    
    # デフォルトのマイクを使用
    with sr.Microphone() as source:
        print("話してください...")
        
        # マイクからの音声を聞き取る
        audio = recognizer.listen(source)
        
        try:
            # Google Speech Recognition APIを使用して音声をテキストに変換
            text = recognizer.recognize_google(audio, language='ja-JP')
            print(f"認識されたテキスト: {text}")
        except sr.UnknownValueError:
            print("音声を認識できませんでした")
        except sr.RequestError as e:
            print(f"音声認識サービスでエラーが発生しました; {e}")

if __name__ == "__main__":
    main()
