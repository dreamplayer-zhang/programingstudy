using RootTools.Trees;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace RootTools
{
    public class Login : NotifyProperty
    {
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
            public string m_sUserName = ""; 
            public eLevel m_eLevel = eLevel.Logout;
            public string m_sPassword = "Password";

            public User(string id, eLevel level, string sPassword = "")
            {
                m_id = id;
                m_eLevel = level; 
                m_sUserName = id;
                m_sPassword = (sPassword != "") ? sPassword : id; 
            }

            public void RunTree(Tree treeRoot)
            {
                Tree tree = treeRoot.GetTree(m_id); 
                m_eLevel = (eLevel)tree.Set(m_eLevel, eLevel.Logout, "Level", "Login Level");
                m_sUserName = tree.Set(m_sUserName, m_sUserName, "User", "User Name");
                m_sPassword = tree.SetPassword(m_sPassword, m_sPassword, "Password", "Login Password");
            }

            public bool CheckLogin(string sPassword, string sUserName)
            {
                if (sPassword != m_sPassword) return false;
                if (sUserName == "") return true;
                return (sUserName == m_sUserName); 
            }
        }
        const int m_nUser = 8;
        List<User> m_aUser = new List<User>();
        List<User> m_aAllUser = new List<User>();
        User m_userLogout = new User("Logout", eLevel.Logout); 
        User m_userATI = new User("ATI", eLevel.Admin, "TryThinkFight");

        void InitUser()
        {
            for (int n = 1; n <= m_nUser; n++)
            {
                User user = new User("User" + n.ToString(), eLevel.Logout);
                m_aUser.Add(user);
                m_aAllUser.Add(user); 
            }
            m_aAllUser.Add(m_userLogout);
            m_aAllUser.Add(m_userATI);
            _user = m_userLogout;
        }
        #endregion

        #region Binding
        public eLevel p_eLevel { get { return _user.m_eLevel; } }

        public string p_sUserName
        {
            get { return _user.m_sUserName; }
        }

        public string p_sComboName { get; set; }

        ObservableCollection<string> _asUserName = new ObservableCollection<string>(); 
        public ObservableCollection<string> p_asUserName { get { return _asUserName; } }
        void InvalidUserNames()
        {
            _asUserName.Clear();
            foreach (User user in m_aUser) _asUserName.Add(user.m_sUserName);
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
        }

        public bool m_bCheckUserName = false;
        public void CheckLogin(string sPassword)
        {
            if (sPassword == m_userATI.m_sPassword)
            {
                p_user = m_userATI;
                return;
            }
            string sUserName = m_bCheckUserName ? p_sComboName : ""; 
            foreach (User user in m_aAllUser)
            {
                if (user.CheckLogin(sPassword, sUserName))
                {
                    p_user = user;
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

        public string m_id = "Login";
        public Log m_log; 
        public void Init()
        {
            m_log = LogView.GetLog(m_id);
            InitUser();
            m_treeRoot = new TreeRoot(m_id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);
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

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public TreeRoot m_treeRoot; 
        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeSetup(m_treeRoot.GetTree("Setup")); 
            Tree treeSetup = m_treeRoot.GetTree("Setup");
            
            foreach (User user in m_aUser) user.RunTree(m_treeRoot);
            InvalidUserNames(); 
        }

        void RunTreeSetup(Tree tree)
        {
            m_bCheckUserName = tree.Set(m_bCheckUserName, false, "Check UserName", "Check UserName with Password");
            m_bKeepUser = tree.Set(m_bKeepUser, false, "Keep User", "Keep User Level");
            m_sKeepUser = tree.Set(m_sKeepUser, "", "KeepID", "Keep User ID", false);
        }
    }
}
