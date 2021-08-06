using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RootTools.Database
{
    public class Lotinfo2
    {
        public DateTime InspectionStart;
        public DateTime InspectionEnd;
        public string sInspectionID;
        public string sLotID;
        public string sCSTID;
        public string sWaferID;
        public string sRecipeID;


        protected string sPartID;
        protected string sSetupID;


        public Lotinfo2()
        {
        }

        public Lotinfo2(string sInspectionID, string lotid, string partid , string setupid , string cstid, string waferid, string recipeid)
        {
            this.sInspectionID = sInspectionID;
            this.sLotID = lotid;
            this.sPartID = partid;
            this.sSetupID = setupid;
            this.sCSTID = cstid;
            this.sWaferID = waferid;
            this.sRecipeID = recipeid;
        }


        public void SetLotinfo(string lotid, string partid, string setupid, string cstid, string waferid, string recipeid)
        {
            sLotID = lotid;
            sPartID = partid;
            sSetupID = setupid;
            sCSTID = cstid;
            sWaferID = waferid;
            sRecipeID = recipeid;
        }

        public string GetLotID() { return sLotID; }
        public string GetCSTID() { return sCSTID; }
        public string GetWaferID() { return sWaferID; }
        public string GetRecipeID() { return sRecipeID; }
    }
}
