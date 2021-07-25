using System;
using System.Collections.Generic;
using PX.Data;
using eGUICustomizations.DAC;
using eGUICustomizations.Descriptor;
using eGUICustomizations.Graph;

namespace eGUICustomizations.Graph
{
    public class TWNExpOnlineStrGUICN : PXGraph<TWNExpOnlineStrGUICN>
    {
        #region Features & Setup
        public PXCancel<TWNGUITrans> Cancel;
        public PXProcessing<TWNGUITrans,
                            Where<TWNGUITrans.eGUIExcluded, Equal<False>,
                                  And<TWNGUITrans.gUIFormatcode, Equal<PX.Objects.AR.ARRegisterExt.VATOut33Att>,
                                       And2<Where<TWNGUITrans.eGUIExported, Equal<False>,
                                                  Or<TWNGUITrans.eGUIExported, IsNull>>,
                                            And<Where<TWNGUITrans.taxNbr, IsNull,
                                                      Or<TWNGUITrans.taxNbr, Equal<StringEmpty>>>>>>>> GUITranProc;
        #endregion

        #region Ctor
        public TWNExpOnlineStrGUICN()
        {
            GUITranProc.SetProcessCaption(ActionsMessages.Upload);
            GUITranProc.SetProcessAllCaption(TWMessages.UploadAll);
            GUITranProc.SetProcessDelegate(TWNExpOnlineStrGUIInv.Upload);
        }
        #endregion 
    }
}