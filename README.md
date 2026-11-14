# Huehop Atlas

Unity 6.4 ile geliştirilmiş 2D hafıza ve rota oyunu. Ana sahneyi `Assets/Scenes/MainScene.unity` yolundan açıp Play'e basarak başlayabilirsin. Oyun çevrimdışı çalışır ve ilerlemeyi cihazda saklar.

İlk açılışta Android, bağlı mobil şebekenin ülkesini kullanır. Şebeke ülkesi yoksa cihazın bölge ayarına bakar. Rusya için Rusça, diğer bölgeler için İngilizce açılır. **Settings / Настройки** ekranında dili istediğin zaman değiştirebilirsin. Seçim cihazda saklanır.

## Oynanış

- Renkleri ve puan işlemlerini ezberle. Sol alttaki **BAŞLA** karesinden sürükleyerek yatay veya dikey bir rota çiz. Son kareye dokunarak veya geri al düğmesiyle rotayı kısalt.
- Küp, **Yolu Onayla** düğmesine basınca kareler arasında zıplar. Çizim süresi dolduğunda en az iki karelik rota varsa oyun onu yürütür. Başlangıç renginde bitmek bölümü açar ve her aynı renkli normal kare, kendi işleminden sonra puanı ikiye katlar.
- Duvarlar geçişi engeller. Dikenler 50 puan götürür. Fırlatıcılar ok yönündeki komşuya iter. Rota bir fırlatıcı üzerinde bitiyorsa gerçek bitiş iniş karesidir. Fırlatıcı, rotandaki daha ilerideki bir kareye kısayol yaparsa atlanan karelerin puan etkileri ve 50 ek puan inişten önce işlenir.
- Kristalleri başarılı bitirişlerle topla. Her beş kristalle bir küp görünümü açılır. Yeniden oynarken her kristal yalnızca bir kez kaydedilir.

## Bölümler ve yıldızlar

| Bölüm | Tahta | Ezber süresi | Çizim süresi |
| --- | --- | --- | --- |
| 1–16 | 4×4 | 8–10 sn | 20–25 sn |
| 17–48 | 5×5 | 10 sn | 27 sn |
| 49–84 | 6×6 | 12 sn | 34 sn |
| 85 ve sonrası | 7×7 | 14 sn | 41 sn |

Bölüm numarası ve kayıt tohumu tahtayı belirler. Tekrar denemelerde aynı tahta açılır. 7×7 boyutundan sonra yeni tahtalar üretilmeye devam eder. Üretici puan getiren en az bir geçerli rota ayırır ve tam aramayı hızlı tutmak için büyük tahtalarda bölgeleri sınırlı geçitlerle bağlar.

Çözücü, çizilebilir rotaları ve fırlatıcı inişlerini tarayarak her tahta için gerçek maksimum puanı hesaplar. İlk bölümde iki yıldız için maksimum puanın en az %12'si, üç yıldız için en az %25'i gerekir. Üç yıldız eşiği ilerleyen bölümlerde %64'e kadar yükselir. İlk sekiz bölümdeki kayıtlı yıldızlar bölüm rekoru üzerinden güncellenir.

## Derleme ve test

Unity Hub'da `6000.4.3f1` kullan. Android için Android Build Support ve beraberindeki SDK, NDK ve OpenJDK modüllerini kur. Unity menüsündeki **Huehop Atlas → Build Android APK** seçeneği `Builds/HuehopAtlas.apk`, **Huehop Atlas → Build Windows** ise `Builds/Windows/HuehopAtlas.exe` üretir. Android paketi ARM64 ve Android 7.1 veya üzerini hedefler.

Unity Test Runner'da **EditMode** ve **PlayMode** testleri vardır. EditMode testleri puanlama, özel kareler, maksimum puan ve kayıt davranışını kontrol eder. PlayMode testleri ana sahnede ilk turu, bölüm açmayı, duraklatmayı ve süre dolduğunda yeniden denemeyi yürütür.

Yazı tipi Inter. SIL Open Font License metni `Assets/Resources/RenkYolu/Fonts` klasöründedir.
