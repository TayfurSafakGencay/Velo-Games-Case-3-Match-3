//Author: Tamer Erdoğan

using Newtonsoft.Json;
using UnityEngine;

public class SaveOnDeviceHelper
{
    public static void SaveUser(UserFD userFD)
    {
        PlayerPrefs.SetString("user", JsonConvert.SerializeObject(userFD));
    }

    public static UserFD GetUser()
    {
        string userJson = PlayerPrefs.GetString("user");
        return JsonConvert.DeserializeObject<UserFD>(userJson);
    }

    public static int AddUserScore(int score)
    {
        UserFD userFD = GetUser();
        userFD.score += score;
        SaveUser(userFD);
        return userFD.score;
    }
}
