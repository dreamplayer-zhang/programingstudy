using RootTools.Trees;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace RootTools
{
    public class Login : NotifyProperty
    {
        #region deligate
        public delegate void dgOnChangeUser();
        public event dgOnChangeUser OnChangeUser;
        #endregion

        public enum eLevel
        {
            Logout,
            Worker,
            Operator,
            Admin,
        }

        #region User
        public class User
        {
            public string m_id;
            public eLevel m_eLevel = eLevel.Logout;
            public string m_sPassword = "Password";
            
            public User(string id, eLevel level, string sPassword = "")
            {
                m_id = id;
                m_eLevel = level; 
                m_sPassword = (sPassword != "") ? sPassword : id;
            }

            public void RunTree(Tree tree)
            {
                m_eLevel = (eLevel)tree.Set(m_eLevel, m_eLevel, "Level", "Login Level");
                m_sPassword = tree.SetPassword(m_sPassword, m_sPassword, "Password", "Login Password");
            }

            public bool CheckLogin(string sPassword, string sUserName)
            {
                if (sPassword != m_sPassword) return false;
                if (sUserName == "") return true;
                return (sUserName == m_id); 
            }
        }
        List<User> m_aUser = new List<User>();
        List<User> m_aAllUser = new List<User>();
        User m_userLogout = new User("Logout", eLevel.Logout); 
        User m_userATI = new User("ATI", eLevel.Admin, "TryThinkFight");
        User m_userOperator = new User("User1", eLevel.Operator, "TryThinkFight");
        User m_userWorker = new User("User2", eLevel.Worker, "TryThinkFight");


        void InitUser()
        {
            AddUser(new User(eLevel.Admin.ToString(), eLevel.Admin));
            AddUser(new User(eLevel.Operator.ToString(), eLevel.Operator));
            AddUser(new User(eLevel.Worker.ToString(), eLevel.Worker));
            m_aAllUser.Add(m_userLogout);
            m_aAllUser.Add(m_userATI);
            m_aAllUser.Add(m_userOperator);
            m_aAllUser.Add(m_userWorker);
            _user = m_userLogout;
        }

        void AddUser(User user)
        {
            m_aUser.Add(user);
            m_aAllUser.Add(user);
        }

        public void AddUser(string sID)
        {
            foreach (User user in m_aUser)
            {
                if (user.m_id == sID) return; 
            }
            AddUser(new User(sID, eLevel.Worker));
            RunTree(Tree.eMode.Init); 
        }

        public void DeleteUser(string sID)
        {
            User userDelete = null; 
            for (int n = 3; n < m_aUser.Count; n++)
            {
                User user = m_aUser[n]; 
                if (user.m_id == sID) userDelete = user;
            }
            if (userDelete != null)
            {
                m_aUser.Remove(userDelete);
                m_aAllUser.Remove(userDelete);
                RunTree(Tree.eMode.Init);
            }
        }
        #endregion

        #region Binding
        public eLevel p_eLevel { get { return _user.m_eLevel; } }

        public string p_sUserName
        {
            get { return _user.m_id; }
        }

        public string p_sComboName { get; set; }

        public ObservableCollection<string> p_asUserName { get; set; }
        
        void InvalidUserNames()
        {
            p_asUserName.Clear();
            foreach (User user in m_aUser) if (user.m_eLevel > eLevel.Logout) p_asUserName.Add(user.m_id);
            OnPropertyChanged("p_asUserName");
        }

        User _user = null; 
        public User p_user
        {
            get { return _user; }
            set
            {
                _user = value;
                OnPropertyChanged("p_sUserName");
                OnPropertyChanged("p_asUserName");
                OnPropertyChanged("p_eLevel");
                OnPropertyChanged("p_colorLevel");
                OnPropertyChanged();
            }
        }

        public Brush p_colorLevel
        {
            get
            {
                switch (p_eLevel)
                {
                    case eLevel.Logout: return Brushes.LightGray;
                    case eLevel.Worker: return Brushes.LightGreen;
                    case eLevel.Operator: return Brushes.LightBlue;
                    case eLevel.Admin: return Brushes.White;
                }
                return Brushes.Yellow;
            }
        }
        #endregion

        #region public
        public void Logout()
        {
            p_user = m_userLogout;
            if (OnChangeUser != null) OnChangeUser();
        }

        public bool m_bCheckUserName = false;
        public void CheckLogin(string sPassword)
        {
            if (sPassword == m_userATI.m_sPassword)
            {
                p_user = m_userATI;
                if (OnChangeUser != null) OnChangeUser();
                return;
            }
            string sUserName = m_bCheckUserName ? p_sComboName : ""; 
            foreach (User user in m_aAllUser)
            {
                if (user.CheckLogin(sPassword, sUserName))
                {
                    p_user = user;
                    if (OnChangeUser != null) OnChangeUser();
                    return; 
                }
            }
        }
        #endregion

        #region KeepUser
        bool m_bKeepUser = false;
        string m_sKeepUser = ""; 
        void KeepUser()
        {
            foreach (User user in m_aAllUser)
            {
                if (user.m_id == m_sKeepUser)
                {
                    p_user = user;
                    return; 
                }
            }
        }
        #endregion

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
            InvalidUserNames();
        }

        public TreeRoot m_treeRoot;
        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeSetup(m_treeRoot.GetTree("Setup"));
            RunTreeUserInfo(m_treeRoot.GetTree("UserInfo", false, false)); 

            Tree treeUser = m_treeRoot.GetTree("User", false);
            foreach (User user in m_aUser) user.RunTree(treeUser.GetTree(user.m_id, false));
        }

        void RunTreeUserInfo(Tree tree)
        {
            int nUser = m_aUser.Count;
            nUser = tree.Set(nUser, nUser, "UserCount", "User Count", false); 
            for (int n = 0; n < nUser; n++)
            {
                string sID = (n < m_aUser.Count) ? m_aUser[n].m_id : "User" + n.ToString();
                sID = tree.Set(sID, sID, "User" + n.ToString(), "User ID", false); 
                if (m_aUser.Count <= n) AddUser(new User(sID, eLevel.Worker));
            }
        }

        void RunTreeSetup(Tree tree)
        {
            m_bCheckUserName = tree.Set(m_bCheckUserName, false, "Check UserName", "Check UserName with Password");
            m_bKeepUser = tree.Set(m_bKeepUser, false, "Keep User", "Keep User Level");
            m_sKeepUser = tree.Set(m_sKeepUser, "", "KeepID", "Keep User ID", false);
        }
        #endregion

        public Login()
        {
            p_asUserName = new ObservableCollection<string>();
        }

        public string m_id = "Login";
        public Log m_log;
        public void Init()
        {
            m_log = LogView.GetLog(m_id);
            InitUser();
            m_treeRoot = new TreeRoot(m_id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);
            InvalidUserNames();
            KeepUser();
        }

        public void ThreadStop()
        {
            if (m_bKeepUser)
            {
                m_sKeepUser = p_user.m_id;
                RunTree(Tree.eMode.RegWrite); 
            }
        }
    }
}
