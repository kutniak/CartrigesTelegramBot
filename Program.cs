using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Data.Entity;

namespace CartrigesTelegramBot
{
    internal class Program
    {
        private static TelegramBotClient bot;
        private static int countFirstSms = 0;
        private static int countNoText = 0;

        static void Main(string[] args)
        {
            bot = new TelegramBotClient("6034187669:AAF2uDnPcV3J2M1BNbVcOUeTRHuhn3U7qtw");
            bot.StartReceiving(Update, Error);
            Console.WriteLine("Бот запущений");
            using (var dbContext = new MyDbContext())
            {
                var cartriges = dbContext.Cartridges.ToList();
                foreach (var e in cartriges)
                {
                    Console.WriteLine($"ID: {e.CartridgeId} || Name: {e.CartridgeName} || Count: {e.CartridgeCount}");
                }
            }
            Console.ReadLine();
        }
        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message;
            if (message.Text != null)
            {
                string firstName;
                if (message.Chat.FirstName == null)
                {
                    firstName = "Anonymous";
                }
                else { firstName = message.Chat.FirstName; }
                if (countFirstSms < 1)
                {
                    FirstSMS(firstName, update, botClient);
                    countFirstSms++;
                }
                else
                {
                    Collection_Information(firstName, update, botClient);
                }
                Console.WriteLine($"{firstName} || {message.Chat.Id} || {message.Date} || {message.Text}");
            }
            else {
                countNoText++;
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Напишіть текстове повідомлення. Данний бот не підтримує інший варіант спілкування. Ви надіслали повідомлення форматом: {message.Type.ToString()}");
                if (countNoText > 2)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "УВАЖНІШЕ ЧИТАЙТЕ КРИТЕРІЇ ПОШУКУ (ТЕКСТ)");
                }

            }
        }

        async static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }

        static private void FirstSMS(string firstName, Update update, ITelegramBotClient client)
        {
            var message = update.Message;
            client.SendTextMessageAsync(message.Chat.Id, $"Привіт, {firstName}! Цей бот для бронювання та інформації про наявність катриджів! Для продовження надішліть 'Так!'");
        }
        public struct UserDialogState
        {
            [Key]
            public string chatId;
            public string FirstName;
            public string LastNameOfTheJudge;
            public string RoomNumber;
            public string NumberСartridge;
        }
        private static readonly Dictionary<long, UserDialogState> userDialogStates = new Dictionary<long, UserDialogState>();
        private static List<string> cartridge = new List<string>() { "725", "85", "35A", "36A", "703", "12A", "FX10", "728", "78", "051", "052", "111", "05", "17A", "30A", "3140", "712" };
        private static bool CartrigeIsTrue(string cartridgeNumber)
        {
            return cartridge.Contains(cartridgeNumber);
        }
        static private async void Collection_Information(string firstName, Update update, ITelegramBotClient client)
        {
            var message = update.Message;
            long chatId = message.Chat.Id;
            long MaxID = 587098913;
            long NastyaID = 1185815551;
            long ChaikaID = 1784785569;
            if (message.Text == "/start")
            {
                if (userDialogStates.ContainsKey(chatId))
                {
                    // Якщо існує, видаляємо його стан діалогу
                    userDialogStates.Remove(chatId);
                }
                await client.SendTextMessageAsync(chatId, $"Привіт, {firstName}! Цей бот для бронювання та інформації про наявність катриджів! Для продовження надішліть 'Так!'");
            }else if (message.Text == "/information")
            {
                using (var dbContext = new MyDbContext())
                {
                    var queryResult = dbContext.Cartridges
                        .Select(cartridge => new
                        {
                            CartrigeName = cartridge.CartridgeName,
                            CartrigeCount = cartridge.CartridgeCount
                        }).ToList();
                    string messageText;
                    if (queryResult.Any())
                    {
                        messageText = "Наявні картриджі";
                        foreach (var cartridge in queryResult)
                        {
                            messageText += $"\nНазва картриджу: {cartridge.CartrigeName}, Кількість картриджів: {cartridge.CartrigeCount}";
                        }
                    }
                    else
                    {
                        messageText = "Нічого не знайдено за заданим запитом.";
                    }
                    await client.SendTextMessageAsync(chatId, messageText);
                }
            }
            else if (!userDialogStates.ContainsKey(chatId))
            {
                userDialogStates[chatId] = new UserDialogState
                {
                    FirstName = firstName,
                };
                await client.SendTextMessageAsync(chatId, "Вкажіть номер кабінету:");
            }
            else
            {
                var dialogState = userDialogStates[chatId];
                if (string.IsNullOrEmpty(dialogState.RoomNumber))
                {
                    dialogState.RoomNumber = message.Text;
                    userDialogStates[chatId] = dialogState;
                    await client.SendTextMessageAsync(chatId, "Вкажіть прізвище судді, якщо кабінет надішліть: '-'");
                }
                else if (string.IsNullOrEmpty(dialogState.LastNameOfTheJudge))
                {
                    dialogState.LastNameOfTheJudge = message.Text;
                    userDialogStates[chatId] = dialogState;
                    await client.SendTextMessageAsync(chatId, "Вкажіть потрібний картридж (725, 85, 35A, 36A, 703, 12A, FX10, 728, 78, 051, 052, 111, 05, 17A, 30A, 3140, 712):");
                }
                else if (string.IsNullOrEmpty(dialogState.NumberСartridge))
                {
                    if(CartrigeIsTrue(message.Text))
                    {
                        dialogState.NumberСartridge = message.Text;
                        userDialogStates[chatId] = dialogState;
                        DateTime currentTime = DateTime.Now;
                        string formattedTime = currentTime.ToString();

                        using (var dbContext = new MyDbContext())
                        {
                            var queryResult = dbContext.Cartridges.Where(cartridge => cartridge.CartridgeName.Contains(message.Text))
                                .Select(cartridge => new
                                {
                                    CartrigeName = cartridge.CartridgeName,
                                    CartrigeCount = cartridge.CartridgeCount
                                }).FirstOrDefault();

                            if (queryResult != null)
                            {
                                string nameCartridge = queryResult.CartrigeName;
                                int countCartridge = queryResult.CartrigeCount;
                                if (countCartridge > 0)
                                {
                                    await client.SendTextMessageAsync(chatId, $"Бронь: Зал: {dialogState.RoomNumber} || Суддя: {dialogState.LastNameOfTheJudge} || Картридж: {dialogState.NumberСartridge} || Час: {formattedTime}. Бронь діє протягом 15хв, після - автоматично анулюється. Для створення нової броні надішліть номер картриджа:");
                                    await client.SendTextMessageAsync(MaxID,    $"Бронь: Ім'я: {message.Chat.FirstName} || Зал: {dialogState.RoomNumber} || Суддя: {dialogState.LastNameOfTheJudge} || Картридж: {dialogState.NumberСartridge} || Кількість: {countCartridge} || Час: {formattedTime}.");
                                    await client.SendTextMessageAsync(NastyaID, $"Бронь: Ім'я: {message.Chat.FirstName} || Зал: {dialogState.RoomNumber} || Суддя: {dialogState.LastNameOfTheJudge} || Картридж: {dialogState.NumberСartridge} || Кількість: {countCartridge} || Час: {formattedTime}.");
                                    await client.SendTextMessageAsync(ChaikaID, $"Бронь: Ім'я: {message.Chat.FirstName} || Зал: {dialogState.RoomNumber} || Суддя: {dialogState.LastNameOfTheJudge} || Картридж: {dialogState.NumberСartridge} || Кількість: {countCartridge} || Час: {formattedTime}.");
                                    Console.WriteLine($"Бронь: Зал: {dialogState.RoomNumber} || Суддя: {dialogState.LastNameOfTheJudge} || Картридж: {dialogState.NumberСartridge} || Кількість: {countCartridge} || Час: {formattedTime}");
                                }
                                else
                                {
                                    await client.SendTextMessageAsync(chatId, $"Данного картриджа немає в наявності. Запитайте пізніше. Можна розпочати одразу з номера картриджу. Для повторного заповнення даних надішліть смс: '/start'");
                                }
                            }
                        }
                    }
                    else
                    {
                        await client.SendTextMessageAsync(chatId, $"Ви ввели неправильний номер картриджу. Перевірте номер та введіть знову");
                    }
                }
                else if (message.Text != null)
                {
                    await client.SendTextMessageAsync(chatId, $"Номер картриджа: {message.Text}");
                    if (message.Text != null)
                    {
                        if(CartrigeIsTrue(message.Text ))
                        {
                            dialogState.NumberСartridge = message.Text;
                            userDialogStates[chatId] = dialogState;
                            DateTime currentTime = DateTime.Now;
                            string formattedTime = currentTime.ToString();

                            using (var dbContext = new MyDbContext())
                            {
                                var queryResult = dbContext.Cartridges.Where(cartridge => cartridge.CartridgeName.Contains(message.Text))
                                    .Select(cartridge => new
                                    {
                                        CartrigeName = cartridge.CartridgeName,
                                        CartrigeCount = cartridge.CartridgeCount
                                    }).FirstOrDefault();

                                if (queryResult != null)
                                {
                                    string nameCartridge = queryResult.CartrigeName;
                                    int countCartridge = queryResult.CartrigeCount;
                                    if (countCartridge > 0)
                                    {
                                        await client.SendTextMessageAsync(chatId, $"Бронь: Зал: {dialogState.RoomNumber} || Суддя: {dialogState.LastNameOfTheJudge} || Картридж: {dialogState.NumberСartridge} || Час: {formattedTime}. Бронь діє протягом 15хв, після - автоматично анулюється. Для створення нової броні надішліть номер картриджа:");
                                        await client.SendTextMessageAsync(MaxID   , $"Бронь: Ім'я: {message.Chat.FirstName} || Зал: {dialogState.RoomNumber} || Суддя: {dialogState.LastNameOfTheJudge} || Картридж: {dialogState.NumberСartridge} || Кількість: {countCartridge} || Час: {formattedTime}.");
                                        await client.SendTextMessageAsync(NastyaID, $"Бронь: Ім'я: {message.Chat.FirstName} || Зал: {dialogState.RoomNumber} || Суддя: {dialogState.LastNameOfTheJudge} || Картридж: {dialogState.NumberСartridge} || Кількість: {countCartridge} || Час: {formattedTime}.");
                                        await client.SendTextMessageAsync(ChaikaID, $"Бронь: Ім'я: {message.Chat.FirstName} || Зал: {dialogState.RoomNumber} || Суддя: {dialogState.LastNameOfTheJudge} || Картридж: {dialogState.NumberСartridge} || Кількість: {countCartridge} || Час: {formattedTime}.");
                                        Console.WriteLine($"Бронь: Зал: {dialogState.RoomNumber} || Суддя: {dialogState.LastNameOfTheJudge} || Картридж: {dialogState.NumberСartridge} || Кількість: {countCartridge} || Час: {formattedTime}");
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(chatId, $"Данного картриджа немає в наявності. Запитайте пізніше.");
                                    }
                                }
                            }
                        }
                        else
                        {
                            await client.SendTextMessageAsync(chatId, $"Ви ввели неправильний номер картриджу. Перевірте номер та введіть знову");
                        }
                    }
                }
            }
        }
    }
}
