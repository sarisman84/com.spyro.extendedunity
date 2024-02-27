using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace Spyro
{
    public delegate void SaveData<TGameData>(ref TGameData data);
    public delegate void LoadData<TGameData>(TGameData data);
    public class SaveSystem<TGameData> where TGameData : class, new()
    {
        class FileHandler
        {
            public static string defaultDirPath = Application.persistentDataPath;

            public static TGameData Load(string dataDirPath, string dataFileName, string encryptionCode = null)
            {
                var fullPath = Path.Combine(dataDirPath, dataFileName);

                TGameData loadedData = null;

                if (File.Exists(fullPath))
                {
                    try
                    {
                        var dataToLoad = string.Empty;
                        using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                dataToLoad = reader.ReadToEnd();
                            }
                        }

                        if (!string.IsNullOrEmpty(encryptionCode))
                        {
                            dataToLoad = EncryptDecrypt(encryptionCode, dataToLoad);
                        }

                        loadedData = JsonUtility.FromJson<TGameData>(dataToLoad);
                    }
                    catch (Exception e)
                    {

                        Debug.LogError($"Error occured when trying to load data to file: {fullPath}\n{e}");
                    }
                }

                return loadedData;
            }


            public static void Save(TGameData data, string dataDirPath, string dataFileName, string encryptionCode = null)
            {
                var fullPath = Path.Combine(dataDirPath, dataFileName);

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                    var dataToStore = JsonUtility.ToJson(data, true);

                    if (!string.IsNullOrEmpty(encryptionCode))
                    {
                        dataToStore = EncryptDecrypt(encryptionCode, dataToStore);
                    }

                    using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.Write(dataToStore);
                        }
                    }
                }
                catch (Exception e)
                {

                    Debug.LogError($"Error occured when trying to save data to file: {fullPath}\n{e}");
                }
            }

            private static string EncryptDecrypt(string code, string data)
            {
                var mod = string.Empty;
                for (int i = 0; i < data.Length; ++i)
                {
                    mod += (char)(data[i] ^ code[i % code.Length]);
                }

                return mod;
            }


            public static string AppendDirectory(string directory)
            {
                return Path.Combine(defaultDirPath, directory);
            }

        }


        private TGameData gameData;

        public event SaveData<TGameData> onGameSave;
        public event LoadData<TGameData> onGameLoad;

        public void NewGame()
        {
            gameData = new TGameData();
        }


        public void LoadGame(string fileName, string fileDirectory, string encryptionCode = null)
        {
            var path = FileHandler.defaultDirPath;
            if (!string.IsNullOrEmpty(fileDirectory))
            {
                path = FileHandler.AppendDirectory(fileDirectory);
            }

            gameData = FileHandler.Load(path, fileName);

            if (gameData == null)
            {
                NewGame();
            }

            if (onGameLoad != null)
                onGameLoad(gameData);
        }

        public void SaveGame(string fileName, string fileDirectory, string encryptionCode = null)
        {
            var path = FileHandler.defaultDirPath;
            if (!string.IsNullOrEmpty(fileDirectory))
            {
                path = FileHandler.AppendDirectory(fileDirectory);
            }

            if (onGameSave != null)
                onGameSave(ref gameData);

            FileHandler.Save(gameData, path, fileName);
        }

    }
}

