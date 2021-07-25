using PX.Data;
using PX.Data.BQL.Fluent;
using eGUICustomizations.DAC;

namespace eGUICustomizations.Graph
{
    public class TWNGUIPrefMaint : PXGraph<TWNGUIPrefMaint>
    { 
        public PXSave<TWNGUIPreferences> Save;
        public PXCancel<TWNGUIPreferences> Cancel;

        public SelectFrom<TWNGUIPreferences>.View GUIPreferences;
    }
}