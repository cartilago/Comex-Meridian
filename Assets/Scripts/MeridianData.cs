using UnityEngine;

public class MeridianData
{
    [System.Serializable]
    public class UserLoginResult
    {
        public UserLogin[] userList;
    }

    [System.Serializable]
    public class UserLogin
    {
        public int Estatus;
        public string Token;
        public string Msj;
        public string UserName;
    }

    [System.Serializable]
    public class RegisterUserResult
    {
        public RegisterUser[] registerUserList;
    }

    [System.Serializable]
    public class RegisterUser
    {
        public int ID;
        public int Estatus;
        public string Token;
        public string Msj;
    }

    [System.Serializable]
    public class Store
    {
        public int ID;
        public string Tienda;
        public string Cadena;
        public string Latitud;
        public string Longitud;
        public string Determinante;

        public Vector2 coordinates
        {
            get
            {
                return new Vector2(float.Parse(Longitud), float.Parse(Latitud));
            }
        }
    }

    [System.Serializable]
    public class Stores
    {
        public Store[] storeList;
    }
}
