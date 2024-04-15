//Author: Tamer Erdoğan

using Firebase.Firestore;

[FirestoreData]
public class UserFD
{
    [FirestoreProperty]
    public string id { get; set; }

    [FirestoreProperty]
    public string email { get; set; }

    [FirestoreProperty]
    public int score { get; set; } = 0;

    [FirestoreProperty]
    public int level { get; set; } = 0;
}
