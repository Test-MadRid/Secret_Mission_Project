using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using System.Threading.Tasks;
using System.Linq;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class FirebaseManager : MonoBehaviour
{
    [Header("Untuk UI")]
    public GameObject MainMenuUI;
    public GameObject LoginUI;
    public GameObject RegisterUI;
    public GameObject UserUI;
    public GameObject LeaderBoardUI;


    [Header("Untuk Login")]
    public TMP_InputField emailLogin;
    public TMP_InputField passwordLogin;
    public Toggle passwordLoginToggle;
    public Text passwordToggleText;

    [SerializeField] private string Username;
    [SerializeField] private string UserID;

    [Header("Untuk Registrasi")]
    public TMP_InputField emailRegister;
    public TMP_InputField usernameRegister;
    public TMP_InputField passwordRegister;
    public TMP_InputField confirmPassword;
    public Toggle passwordRegisterToggle;
    public Text passwordRegisterToggleText;

    [Header("Untuk User Menu")] //usermenu stuff
    public TMP_Text UserOnUI;
    public TMP_InputField stats1;
    public TMP_InputField stats2;
    public TMP_InputField stats3;
    public Transform LeaderBoardContent;
    public GameObject LeaderBoardElement;

    [Header("Untuk Sudah Login")] //sudahLogin stuff
    public TMP_Text User;

    [Header("Untuk Firebase")]
    public DependencyStatus depStatus;
    public FirebaseUser FirebaseUser;
    public FirebaseAuth FirebaseAuth;
    public DatabaseReference FbDatabase;


    private void Awake()
    {
        passwordToggleText = passwordLoginToggle.GetComponentInChildren<Text>(); //ambilkomponen teks toggle di children
        passwordRegisterToggleText = passwordRegisterToggle.GetComponentInChildren<Text>(); //ambil komponen teks toggle di children

        StartCoroutine(CheckFirebase()); // start coroutine (nama fungsi) adalah syntax untuk memanggil fungsi bertipe IENumerator
    }


    void Start()
    {
        MainMenuUI.SetActive(true);
        LoginUI.SetActive(false);
        UserUI.SetActive(false);
        RegisterUI.SetActive(false);
        LeaderBoardUI.SetActive(false);

        passwordLoginToggle.isOn = false; //toggle ga di centang
        passwordToggleText.text = "Show Password"; //ngubah komponen teks toggle
        passwordLogin.contentType = TMP_InputField.ContentType.Password; //konten input password bintang

        passwordRegisterToggle.isOn = false;
        passwordRegisterToggleText.text = "Show Password";
        passwordRegister.contentType = TMP_InputField.ContentType.Password;
        confirmPassword.contentType = TMP_InputField.ContentType.Password;

    }


    private IEnumerator CheckFirebase()
    {
        Task<DependencyStatus> depTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => depTask.IsCompleted);

        depStatus = depTask.Result;

        if (depStatus == DependencyStatus.Available) // kalau firebase online
        {
            Debug.Log("Firebase online dan bisa digunakan");
            InitializeFirebase();
            yield return new WaitForEndOfFrame();

            /*AutoLogin();*/
            StartCoroutine(AutoLoginCheck());
        }

        else
        {
            Debug.Log("Firebase offline");
        }

    }
    public void InitializeFirebase()
    {
        FirebaseAuth = FirebaseAuth.DefaultInstance;
        FirebaseAuth.StateChanged += AuthStateChanged;
        FbDatabase = FirebaseDatabase.DefaultInstance.RootReference;
        AuthStateChanged(this, null);
    }


    public void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (FirebaseAuth.CurrentUser != FirebaseUser)
        {
            bool signedin = FirebaseUser != FirebaseAuth.CurrentUser && FirebaseAuth.CurrentUser != null;

            if (!signedin && FirebaseUser != null)
            {
                Debug.Log("Signed out " + FirebaseUser.UserId);
            }

            FirebaseUser = FirebaseAuth.CurrentUser;

            if (signedin)
            {
                Debug.Log("Signed in " + FirebaseUser.UserId);
            }
        }
    }

    private IEnumerator AutoLoginCheck()
    {
        if (FirebaseUser != null)
        {
            Task reloadUserTask = FirebaseUser.ReloadAsync(); // untuk memastikan usernya ada
            yield return new WaitUntil(() => reloadUserTask.IsCompleted); // membaca ulang data user dari console firebase

            AutoLogin();
        }
    }

    public void AutoLogin()
    {
        if (FirebaseUser != null) // ekstra pengecekan untuk lebih memastikan user
        {
            UserID = FirebaseUser.UserId;
            Debug.Log("Auto Login Success!!");
            StartCoroutine(AutoLoginTransition());
        }

        else
        {
            MainMenuUI.SetActive(false);
            LoginUI.SetActive(true);
            //dibawa ke loginUI jika autologin gagal
        }
    }

    private IEnumerator AutoLoginTransition()
    {
        yield return new WaitForSeconds(0.8f);

        string u = PlayerPrefs.GetString("myUsername");
        User.text = u; // mengambil string dari player preferences
        Debug.Log(User.text);

        MainMenuUI.SetActive(false);
        UserUI.SetActive(true);

        LoadUserData();

       
        //dibawa ke sudah login
    }

    public void LoadUserData()
    {
        StartCoroutine(LoadUserDataFirebase());
    }
    private IEnumerator LoadUserDataFirebase()
    {
        string userdb = PlayerPrefs.GetString("myUsername");
        Task<DataSnapshot> dbTask = FbDatabase.Child("users").Child(UserID).GetValueAsync();
        yield return new WaitUntil(predicate: () => dbTask.IsCompleted);

        if (dbTask.Exception != null)
        {
            Debug.Log("gagal membaca database");
        }

        else if (dbTask == null)
        {
            Debug.Log("belum ada data tercatat");
            stats1.text = "0";
            stats2.text = "0";
            stats3.text = "0";
        }

        else
        {
            Debug.Log("database berhasil dimuat");
            DataSnapshot snapshot = dbTask.Result;

            stats1.text = snapshot.Child("victory").Value.ToString();
            if (string.IsNullOrEmpty(stats1.text))
            {
                stats1.text = "0";
            }

            stats2.text = snapshot.Child("defeat").Value.ToString();
            if (string.IsNullOrEmpty(stats2.text))
            {
                stats2.text = "0";
            }

            stats3.text = snapshot.Child("experience").Value.ToString();
            if (string.IsNullOrEmpty(stats3.text))
            {
                stats3.text = "0";
            }
        }
    }


    private IEnumerator UpdateUserAuth()
    {
        string userAuth = PlayerPrefs.GetString("myUsername");
        UserProfile profile = new UserProfile { DisplayName = userAuth };

        Task ProfileTask = FirebaseUser.UpdateUserProfileAsync(profile);
        yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

        if (ProfileTask.Exception != null) // kalau ada error
        {
            Debug.Log("Pengecekan User Gagal :(");
        }

        else
        {
            Debug.Log("Pengecekan User Berhasil");
        }

    }

    private IEnumerator UpdateUserDatabase()
    {
/*        string userDB = PlayerPrefs.GetString("myUsername");
        string useridDB = PlayerPrefs.GetString("myUserid");*/

        Task dbTask = FbDatabase.Child("users").Child(UserID).Child("username").SetValueAsync(Username);
        yield return new WaitUntil(predicate: () => dbTask.IsCompleted);

        if (dbTask.Exception != null) // kalau ada error
        {
            Debug.Log("Pengecekan database gagal :(");
        }

        else
        {
            Debug.Log("Pengecekan database user berhasil");
        }

    }

    private IEnumerator UpdateStat1(int s1)
    {
/*        string useridDB = PlayerPrefs.GetString("myUserid");*/
        Task dbTask = FbDatabase.Child("users").Child(UserID).Child("victory").SetValueAsync(s1);
        yield return new WaitUntil(predicate: ()=> dbTask.IsCompleted);

        if (dbTask.Exception != null) // kalau pengiriman data gagal
        {
            Debug.Log("updata database gagal :(");
        }

        else
        {
            Debug.Log("update database berhasil!");
        }
    }

    private IEnumerator UpdateStat2(int s2)
    {
/*        string useridDB = PlayerPrefs.GetString("myUserid");*/
        Task dbTask = FbDatabase.Child("users").Child(UserID).Child("defeat").SetValueAsync(s2);
        yield return new WaitUntil(predicate: () => dbTask.IsCompleted);

        if (dbTask.Exception != null) // kalau pengiriman data gagal
        {
            Debug.Log("updata database gagal :(");
        }

        else
        {
            Debug.Log("update database berhasil!");
        }
    }

    private IEnumerator UpdateStat3(int s3)
    {
/*        string useridDB = PlayerPrefs.GetString("myUserid");*/
        Task dbTask = FbDatabase.Child("users").Child(UserID).Child("experience").SetValueAsync(s3);
        yield return new WaitUntil(predicate: () => dbTask.IsCompleted);

        if (dbTask.Exception != null) // kalau pengiriman data gagal
        {
            Debug.Log("updata database gagal :(");
        }

        else
        {
            Debug.Log("update database berhasil!");
        }
    }


    public void SaveUserData()
    {
/*        string username = PlayerPrefs.GetString("myUsername");*/

        Debug.Log("user yang bersangkutan: " + Username);
        Debug.Log("nilai victory yang akan dikirim: " + stats1.text);
        Debug.Log("nilai defeat yang akan dikirim: " + stats2.text);
        Debug.Log("nilai experience yang akan dikirim: " + stats3.text);

        StartCoroutine(UpdateUserAuth());
        StartCoroutine(UpdateUserDatabase());

        StartCoroutine(UpdateStat1(int.Parse(stats1.text)));
        StartCoroutine(UpdateStat2(int.Parse(stats2.text)));
        StartCoroutine(UpdateStat3(int.Parse(stats3.text)));
    }
    public void ResetUserDataButton()
    {
        StartCoroutine(UpdateStat1(0));
        StartCoroutine(UpdateStat2(0));
        StartCoroutine(UpdateStat3(0));

        LoadUserData();
    }

    public void ShowLeaderBoardButton()
    {
        StartCoroutine(LoadLeaderBoardFirebase());
    }

    private IEnumerator LoadLeaderBoardFirebase()
    {
        Task <DataSnapshot> LBTask = FbDatabase.Child("users").OrderByChild("experience").GetValueAsync();
        yield return new WaitUntil(predicate: () => LBTask.IsCompleted);

        if (LBTask.Exception != null)
        {
            Debug.Log("Failed to load leaderboard");
        }

        else
        {
            Debug.Log("Leader board succesfully loaded");
            DataSnapshot snapshot = LBTask.Result;

            foreach (Transform tableContent in LeaderBoardContent.transform)
            {
                Destroy(tableContent.gameObject);
            }

            foreach (DataSnapshot tableSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                string username = tableSnapshot.Child("username").Value.ToString();
                int stat1 = int.Parse(tableSnapshot.Child("victory").Value.ToString());
                int stat2 = int.Parse(tableSnapshot.Child("defeat").Value.ToString());
                int stat3 = int.Parse(tableSnapshot.Child("experience").Value.ToString());

                GameObject LBE = Instantiate(LeaderBoardElement, LeaderBoardContent);
                LBE.GetComponent<LeaderBoardElement>().New_LBElement(username, stat1, stat2, stat3);
            }

            UserUI.SetActive(false);
            LeaderBoardUI.SetActive(true);
        }
    }


    public void RegisterButton()
    {
        StartCoroutine(RegisterFirebase(emailRegister.text, usernameRegister.text, passwordRegister.text, confirmPassword.text));
    }

    private IEnumerator RegisterFirebase(string email, string username, string password, string confirmPass)
    {
        if (string.IsNullOrEmpty(email)) // cek apakah input teks kosong, true kalau kosong
        {
            Debug.Log("emailnya kosong bos");
        }

        else if (string.IsNullOrEmpty(username)) // cek apakah input teks kosong, true kalau kosong
        {
            Debug.Log("username kosong bos");
        }

        else if (password != confirmPass) // cek apakah konfirmasi password sudah cocok
        {
            Debug.Log("pastikan password cocok");
        }

        else
        {
            Task<AuthResult> registerTask = FirebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password); // daftar menggunakan email, password
            yield return new WaitUntil(() => registerTask.IsCompleted); // tunggu register sampai selesai

            if (registerTask.Exception != null) // kalau register task ada error
            {
                Debug.Log(registerTask.Exception);

                FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException; // mendeklarasi errornya apa
                AuthError authError = (AuthError)firebaseException.ErrorCode; // mengambil error code

                string failMessage = "Registration failed! Because ";

                switch (authError) // switch case itu mirip dgn if else atau if else if
                {
                    case AuthError.InvalidEmail:
                        failMessage += "Email is Invalid";
                        break;
                    case AuthError.WrongPassword:
                        failMessage += "Wrong Password";
                        break;
                    case AuthError.MissingEmail:
                        failMessage += "Email is missing, please provide email";
                        break;
                    case AuthError.MissingPassword:
                        failMessage += "Password is missing, please provide password";
                        break;
                    default:
                        failMessage = "Registration failed :(";
                        break;
                }
                Debug.Log(failMessage);
            }

            else // kalau register task tidak ada error{
            {
                FirebaseUser = registerTask.Result.User; // mengambil user dari hasil register task
                UserProfile uProfile = new UserProfile { DisplayName = username };

                Task ProfileTask = FirebaseUser.UpdateUserProfileAsync(uProfile); // update user
                yield return new WaitUntil(() => ProfileTask.IsCompleted); // tunggu sampai update profile selesai

                Debug.Log(FirebaseUser.DisplayName);
                if (ProfileTask.Exception != null) // kalau update profil user ada error
                {
                    FirebaseUser.DeleteAsync();
                    Debug.Log(ProfileTask.Exception);
                    FirebaseException firebaseException = ProfileTask.Exception.GetBaseException() as FirebaseException;
                    AuthError authError = (AuthError)firebaseException.ErrorCode;

                    string failMessage = "Profile update failed! Because ";
                    switch (authError)
                    {
                        case AuthError.InvalidEmail:
                            failMessage += "Email is Invalid";
                            break;
                        case AuthError.WrongPassword:
                            failMessage += "Wrong Password";
                            break;
                        case AuthError.MissingEmail:
                            failMessage += "Email is missing, please provide email";
                            break;
                        case AuthError.MissingPassword:
                            failMessage += "Password is missing, please provide password";
                            break;
                        default:
                            failMessage = "Profile update failed :(";
                            break;
                    }
                    Debug.Log(failMessage);
                }

                else // kalau tidak ada error
                {
                    Debug.Log(FirebaseUser.DisplayName);
                    Debug.Log("Registrasi berhasil bos"); // registrasi berhasil

                    StartCoroutine(RegisterSuccess()); // menuju halaman login
                }
            }
        }
    }

    private IEnumerator RegisterSuccess()
    {
        yield return new WaitForSeconds(0.8f);

        RegisterUI.SetActive(false);
        LoginUI.SetActive(true);
    }

    public void LoginButton()
    {
        StartCoroutine(LoginFirebase(emailLogin.text, passwordLogin.text));
    }

    private IEnumerator LoginFirebase(string email, string password)
    {
        if (string.IsNullOrEmpty(email)) // kalau kosong (true), error
        {
            Debug.Log("email nya kosong bos");
        }

        else if (string.IsNullOrEmpty(password)) // kalau kosong (true), error
        {
            Debug.Log("password nya kosong");
        }

        else
        {
            Task<AuthResult> loginTask = FirebaseAuth.SignInWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => loginTask.IsCompleted);

            if (loginTask.Exception != null) // kalau ada error
            {
                Debug.Log(loginTask.Exception);

                FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
                AuthError authError = (AuthError)firebaseException.ErrorCode;

                string failMessage = "Login failed! Because ";

                switch (authError) // switch case - mirip if else if
                {
                    case AuthError.InvalidEmail:
                        failMessage += "Email is Invalid";
                        break;
                    case AuthError.WrongPassword:
                        failMessage += "Wrong Password";
                        break;
                    case AuthError.MissingEmail:
                        failMessage += "Email is missing, please provide email";
                        break;
                    case AuthError.MissingPassword:
                        failMessage += "Password is missing, please provide password";
                        break;
                    default:
                        failMessage = "Login failed, try again :(";
                        break;
                }
                Debug.Log(failMessage);
            }

            else
            {
                FirebaseUser = loginTask.Result.User;
                Debug.Log("Login Success! " + FirebaseUser.DisplayName);

                Username = FirebaseUser.DisplayName;
                Debug.Log("Your username: " + Username);
                /*                PlayerPrefs.SetString("myUsername", Username);*/
                PlayerPrefs.SetString("myUsername", Username);
                UserID = FirebaseUser.UserId;
                Debug.Log("Your userid: " + UserID);
                PlayerPrefs.SetString("myUserid", UserID);

                StartCoroutine(LoginSuccess());

/*                SceneManager.LoadScene("Menu");*/
            }
        }
    }

    private IEnumerator LoginSuccess()
    {
        yield return new WaitForSeconds(0.8f);

        LoginUI.SetActive(false);
        UserUI.SetActive(true);


        emailLogin.text = ""; // kosongin inputfield
        passwordLogin.text = ""; // kosongin inputfield

        string u = PlayerPrefs.GetString("myUsername");
        User.text = u;
        Debug.Log(User.text);

        LoadUserData();
    }

    public void LogoutButton()
    {
        FirebaseAuth.SignOut();

        StartCoroutine(LogoutFirebase());
    }


    public void StartGame()
    {
        PhotonNetwork.LoadLevel(1);

    }
    private IEnumerator LogoutFirebase()
    {
        yield return new WaitForSeconds(1.2f);

        UserUI.SetActive(false);
        MainMenuUI.SetActive(true);
    }


    // Update is called once per frame
    void Update()
    {
        if (passwordLoginToggle.isOn)
        {
            passwordToggleText.text = "Hide password";
            passwordLogin.contentType = TMP_InputField.ContentType.Standard;
        }
        else if (!passwordLoginToggle.isOn)
        {
            passwordToggleText.text = "Show password";
            passwordLogin.contentType = TMP_InputField.ContentType.Password;
        }

        passwordLogin.ForceLabelUpdate();


        if (passwordRegisterToggle.isOn)
        {
            passwordRegisterToggleText.text = "Hide password";
            passwordRegister.contentType = TMP_InputField.ContentType.Standard;
            confirmPassword.contentType =TMP_InputField.ContentType.Standard;
            

        }
        else if (!passwordRegisterToggle.isOn)  
        {
            passwordRegisterToggleText.text = " Show password";
            passwordRegister.contentType = TMP_InputField.ContentType.Password;
            confirmPassword.contentType = TMP_InputField.ContentType.Password;
            

        }
        passwordRegister.ForceLabelUpdate();
        confirmPassword.ForceLabelUpdate();
    }
}
