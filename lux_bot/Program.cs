using System;
using System.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace ConsoleApp1
{
    class Program
    {
        private static int Sib = 0; private static int Mosc = 0; private static int Dolb = 0;
        private static long waitingForPhotoFromChatId = 0;
        private static string usersSelectedProcessingMethod = "0";
        private static Dictionary<long, string> userAnswers = new Dictionary<long, string>();

        static void Main(string[] args)
        {
           
            var client = new TelegramBotClient("6753716719:AAHROXUcs9FdUOS-j4hW5Re2N__Mlw9sfyI");
            client.StartReceiving(Update, Error);
            Console.ReadLine();
        }

        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message;
            var callbackQuery = update.CallbackQuery;
            if (message != null)
            {
                if (message.Text != null)
                {
                    Console.WriteLine($"Получено новое сообщение от {message.Chat.FirstName}: {message.Text} в {message.Date}");

                    if (message.Text == "/start")
                    {
                        var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
                        {
                        new[]
                        {
                            new KeyboardButton("Сделать фото черно-белым"),
                            new KeyboardButton("Тест: На каких ты люксах?")
                        },
                        new[]
                        {
                            new KeyboardButton("Что здесь "),
                            new KeyboardButton("Будет ")
                        }
                         })
                        {
                            ResizeKeyboard = true
                        };
                        var channelLink = "https://t.me/luxovie";
                        var inlineKeyboardMarkup = new InlineKeyboardMarkup(
                        new[]
                        {
                          InlineKeyboardButton.WithUrl("Luxовые сучки", channelLink)
                         });
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Приветствую!", replyMarkup: replyKeyboardMarkup);
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Сделайте свой выбор и не забудьте подписаться на наш тг канал!", replyMarkup: inlineKeyboardMarkup);

                    }
                    else if (message.Text.ToLower().Contains("я люксовая сучка"))
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Привет, красотка!");
                        return;
                    }
                    if (message.Text.ToLower().Contains("сделать фото черно-белым"))
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Отправьте фото!");
                        usersSelectedProcessingMethod = "BW";

                    }

                    if (message.Text.ToLower().Contains("тест: на каких ты люксах?"))
                    {
                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                           { new[]{
                                         InlineKeyboardButton.WithCallbackData("1", "drink"),
                                         InlineKeyboardButton.WithCallbackData("2", "eat"),
                                         InlineKeyboardButton.WithCallbackData("3", "food")
                           }
                           });
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Вопрос 1: Хочешь есть?\n 1 - Попей водички\n 2 - Поешь\n 3 - Еда - навязанная обществом бессмысленная концепция", replyMarkup: inlineKeyboard);

                    }
                }



                if (message.Type == MessageType.Photo)
                {
                    Console.WriteLine($"Получено фото от {message.Chat.FirstName} в {message.Date}");

                    var fileId = update.Message.Photo.LastOrDefault()?.FileId;
                    var fileInfo = await botClient.GetFileAsync(fileId);
                    var filePath = fileInfo.FilePath;

                    // Получаем выбранный пользователем метод обработки фото
                    var selectedProcessingMethod = usersSelectedProcessingMethod;

                    // Обработка фото в соответствии с выбранным методом
                    if (selectedProcessingMethod == "BW")
                    {
                        string destinationFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{message.Photo[0].FileUniqueId}.png";


                        await using (Stream fileStream = System.IO.File.Create(destinationFilePath))
                        {
                            var file = await botClient.GetFileAsync(message.Photo.LastOrDefault()?.FileId);
                            await botClient.DownloadFileAsync(file.FilePath, fileStream);
                        }

                        string newFilePath = destinationFilePath.Replace(".png", "_edited.png");

                        using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(destinationFilePath))
                        {
                            image.Mutate(x => x.Grayscale()); // Применяем черно-белый фильтр

                            var jpegEncoder = new JpegEncoder
                            {
                                Quality = 100 // Установка уровня качества сохранения в 100
                            };
                            image.Save(newFilePath, jpegEncoder);
                        }
                        using (Stream photoStream = System.IO.File.OpenRead(newFilePath))
                        {
                            await botClient.SendPhotoAsync(message.Chat.Id, photoStream);
                        }

                        System.IO.File.Delete(destinationFilePath);
                        System.IO.File.Delete(newFilePath);

                        await botClient.SendTextMessageAsync(message.Chat.Id, "Фото стало черно-белым!");
                    }
                    else if (selectedProcessingMethod == "Метод обработки 2")
                    {
                        // Обработка фото по методу 2
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Фото успешно обработано по методу 2!");
                    }

                    // Сброс переменных
                    waitingForPhotoFromChatId = 0;
                    usersSelectedProcessingMethod = "0";
                }
            }

                if (callbackQuery != null)
                {
                    if (callbackQuery.Data == "drink" || callbackQuery.Data == "eat" || callbackQuery.Data == "food")
                {
                    Console.WriteLine($"Получен ответ на 1 вопрос от {callbackQuery.Message.Chat.FirstName}");
                    if (callbackQuery.Data == "drink")
                        {
                            Dolb++;
                        }
                        else if (callbackQuery.Data == "eat")
                        {
                            Mosc++;
                        }
                        else if (callbackQuery.Data == "food")
                        {
                            Sib++;
                        }
                        var inlineKeyboardMarkup = new InlineKeyboardMarkup(new[]
                        {
                        InlineKeyboardButton.WithCallbackData("1", "cucumber"),
                        InlineKeyboardButton.WithCallbackData("2", "turkish"),
                        InlineKeyboardButton.WithCallbackData("3", "chocolate")
                     });
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Вопрос 2: Жадина-говядина...\n 1 - Соленый огурец\n 2 - Турецкий Барабан\n 3 - Пустая шоколадина", replyMarkup: inlineKeyboardMarkup);

                    }
                    if (callbackQuery.Data == "cucumber" || callbackQuery.Data == "turkish" || callbackQuery.Data == "chocolate")
                {
                    Console.WriteLine($"Получен ответ на 2 вопрос от {callbackQuery.Message.Chat.FirstName}");
                    if (callbackQuery.Data == "chocolate")
                        {
                            Dolb++;
                        }
                        else if (callbackQuery.Data == "turkish")
                        {
                            Mosc++;
                        }
                        else if (callbackQuery.Data == "cucumber")
                        {
                            Sib++;
                        }
                        var inlineKeyboardMarkup = new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithCallbackData("1", "proga"),
                            InlineKeyboardButton.WithCallbackData("2", "huega"),
                            InlineKeyboardButton.WithCallbackData("3", "brain")
                         });
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Вопрос 3: Основные принципы ООП???\n 1 -Наследование, инкапсуляция, полиморфизм и абстракция\n 2 - Кручу верчу пентагон взломать хочу\n 3 - Причем тут люкс и ООП...", replyMarkup: inlineKeyboardMarkup);

                    }
                if (callbackQuery.Data == "proga" || callbackQuery.Data == "huega" || callbackQuery.Data == "brain")
                {
                    Console.WriteLine($"Получен ответ на 3 вопрос от {callbackQuery.Message.Chat.FirstName}");
                    if (callbackQuery.Data == "huega")
                    {
                        Dolb++;
                    }
                    else if (callbackQuery.Data == "proga")
                    {
                        Mosc++;
                    }
                    else if (callbackQuery.Data == "brain")
                    {
                        Sib++;
                    }

                    var inlineKeyboardMarkup = new InlineKeyboardMarkup(new[]
                    {
                            InlineKeyboardButton.WithCallbackData("1", "violet"),
                            InlineKeyboardButton.WithCallbackData("2", "gray"),
                            InlineKeyboardButton.WithCallbackData("3", "depression"),
                            InlineKeyboardButton.WithCallbackData("4", "green")
                         });
                    using (Stream photoStream = System.IO.File.OpenRead(@"C:\Users\Junior\Desktop\example.jpg"))
                    {
                        await botClient.SendPhotoAsync(callbackQuery.Message.Chat.Id, photoStream);
                    }
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Вопрос 4: Какой это цвет?\n 1 - Фиолетовый\n 2 - Серо-буро-малиновый\n 3 - Цвет одиночества и жалких попыток выразить свою индивидуальность\n 4 - зеленый", replyMarkup: inlineKeyboardMarkup);

                }
                if (callbackQuery.Data == "green")
                {
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Ты что, совсем дебил?");

                    var inlineKeyboardMarkup = new InlineKeyboardMarkup(new[]
                   {
                            InlineKeyboardButton.WithCallbackData("1", "violet"),
                            InlineKeyboardButton.WithCallbackData("2", "gray"),
                            InlineKeyboardButton.WithCallbackData("3", "depression"),
                            InlineKeyboardButton.WithCallbackData("4", "green")
                         });
                    using (Stream photoStream = System.IO.File.OpenRead(@"C:\Users\Junior\Desktop\example.jpg"))
                    {
                        await botClient.SendPhotoAsync(callbackQuery.Message.Chat.Id, photoStream);
                    }
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Вопрос 4: Какой это цвет?\n 1 - Фиолетовый\n 2 - Серо-буро-малиновый\n 3 - Цвет одиночества и жалких попыток выразить свою индивидуальность\n 4 - Зеленый", replyMarkup: inlineKeyboardMarkup);

                }
                if (callbackQuery.Data == "violet" || callbackQuery.Data == "gray" || callbackQuery.Data == "depression")
                {
                    Console.WriteLine($"Получен ответ на 4 вопрос от {callbackQuery.Message.Chat.FirstName}");
                    if (callbackQuery.Data == "violet")
                    {
                        Dolb++;
                    }
                    else if (callbackQuery.Data == "gray")
                    {
                        Mosc++;
                    }
                    else if (callbackQuery.Data == "depression")
                    {
                        Sib++;
                    }

                    var inlineKeyboardMarkup = new InlineKeyboardMarkup(new[]
                    {
                            InlineKeyboardButton.WithCallbackData("1", "ariana"),
                            InlineKeyboardButton.WithCallbackData("2", "romcom"),
                            InlineKeyboardButton.WithCallbackData("3", "dusnila")
                         });
                    Console.WriteLine($"У {callbackQuery.Message.Chat.FirstName} Sib = {Sib} Dolb = {Dolb} Mosc = {Mosc}");
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Вопрос 5: Твои глаза, как...\n 1 - в рекламе Виши\n 2 - самое прекрасное, что я видел на свете\n 3 - глаза, чего выдумывать то", replyMarkup: inlineKeyboardMarkup);

                }
                if (callbackQuery.Data == "ariana" || callbackQuery.Data == "romcom" || callbackQuery.Data == "dusnila")
                {
                    Console.WriteLine($"Получен ответ на 5 вопрос от {callbackQuery.Message.Chat.FirstName}");
                    if (callbackQuery.Data == "dusnila")
                    {
                        Dolb++;
                    }
                    else if (callbackQuery.Data == "romcom")
                    {
                        Mosc++;
                    }
                    else if (callbackQuery.Data == "ariana")
                    {
                        Sib++;
                    }
                    Console.WriteLine($"У {callbackQuery.Message.Chat.FirstName} Sib = {Sib} Dolb = {Dolb} Mosc = {Mosc}");
                    if (Dolb > Mosc && Dolb > Sib)
                    {
                        Console.WriteLine("Выпал первый вариант");
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Вы прошли тест! Ваш результат:");
                        using (Stream photoStream = System.IO.File.OpenRead(@"C:\Users\Junior\Desktop\var1.jpg"))
                        {
                            await botClient.SendPhotoAsync(callbackQuery.Message.Chat.Id, photoStream);
                        }
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Вы сидите на очень тяжелых люксах. Удачи с кодированием");
                    }
                    if (Mosc > Dolb && Mosc > Sib)
                    {
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Вы прошли тест! Ваш результат:");
                        using (Stream photoStream = System.IO.File.OpenRead(@"C:\Users\Junior\Desktop\var2.jpg"))
                        {
                            await botClient.SendPhotoAsync(callbackQuery.Message.Chat.Id, photoStream);
                        }
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Она выглядит как mommy, среди всех в этом клабе\r\nЕё вид всегда шикарен, на ней сидит всё идеально\r\nУ неё всегда всё, окей, ты должен ей открыть дверь\r\nСумка Birkin, она твёркает, что ты скажешь ей теперь?");
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Короче ебанутые люксы");
                    }
                    if (Sib > Mosc && Sib > Dolb)
                    {
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Вы прошли тест! Ваш результат:");
                        using (Stream photoStream = System.IO.File.OpenRead(@"C:\Users\Junior\Desktop\var3.jpg"))
                        {
                            await botClient.SendPhotoAsync(callbackQuery.Message.Chat.Id, photoStream);
                        }
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Вот она, настоящая slavic girl. И люксы у вас самые slavic");
                    }
                    if (Sib == Mosc && Sib > Dolb)
                    {
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Вы прошли тест! Ваш результат:");
                        using (Stream photoStream = System.IO.File.OpenRead(@"C:\Users\Junior\Desktop\var4.jpg"))
                        {
                            await botClient.SendPhotoAsync(callbackQuery.Message.Chat.Id, photoStream);
                        }
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "О, Ваше Высочество, вы имеете по-настоящему изысканный вкус! Таких изящных люксов я еще ни у кого не видела...");
                    }
                    if (Dolb == Mosc && Dolb > Sib)
                    {
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Вы прошли тест! Ваш результат:");
                        using (Stream photoStream = System.IO.File.OpenRead(@"C:\Users\Junior\Desktop\var5.jpg"))
                        {
                            await botClient.SendPhotoAsync(callbackQuery.Message.Chat.Id, photoStream);
                        }
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Ваши люксы в простонародье зовутся понтами");
                    }
                    if (Dolb == Sib && Dolb > Mosc)
                    {
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Вы прошли тест! Ваш результат:");
                        using (Stream photoStream = System.IO.File.OpenRead(@"C:\Users\Junior\Desktop\var6.jpg"))
                        {
                            await botClient.SendPhotoAsync(callbackQuery.Message.Chat.Id, photoStream);
                        }
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "У вас весьма странный вкус на люкс...");
                    }
                      
                }
            }
            
        }

                private static Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
                {
                    var message = exception.Message;
                    throw new Exception(message);
                }
    }
}