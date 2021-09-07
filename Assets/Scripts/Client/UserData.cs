using System.Collections;
using System.Collections.Generic;

public class UserData
{
    private static string user_id = null;
    private static string user_nickname = null;
    private static string room_id = null;
    private static string room_pw = null;
    private static bool room_master = false;
    private static string userData = null;
    //private static string server_url = null;

    //userID
    public static void Set_userID(string t)
    {
        user_id = t;
    }
    public static string Get_userID()
    {
        return user_id;
    }

    //Nickname
    public static void Set_userNickname(string t)
    {
        user_nickname = t;
    }
    public static string Get_userNickname()
    {
        return user_nickname;
    }

    //Room ID
    public static void Set_roomID(string t)
    {
        room_id = t;
    }
    public static string Get_roomID()
    {
        return room_id;
    }

    //Room PW
    public static void Set_RoomPW(string t)
    {
        room_pw = t;
    }
    public static string Get_RoomPW()
    {
        return room_pw;
    }

    //Room Master
    public static void Set_RoomMaster(bool b)
    {
        room_master = b;
    }
    public static bool Get_RoomMater()
    {
        return room_master;
    }

    //userData
    public static void Set_UserData(string t)
    {
        userData = t;
    }
    public static string Get_UserData()
    {
        return userData;
    }
    //ServerURL
    //public static void Set_serverURL(string t) {
    //    server_url = t;
    //}
    //public static string Get_serverURL()
    //{
    //    return server_url;
    //}
}
