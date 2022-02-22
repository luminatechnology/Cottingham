using PX.Data;
using PX.Data.WorkflowAPI;
using PX.Objects.AR;
using System.Collections;
using System.Collections.Generic;
using eGUICustomizations.Descriptor;

namespace PX.Objects.SO
{
    public class SOInvoiceEntry_Extension : PXGraphExtension<SOInvoiceEntry_Workflow, SOInvoiceEntry>
    {
        public const string GUIReportID = "TW641000";

        #region Override Methods
        public override void Configure(PXScreenConfiguration config)
        {
            Configure(config.GetScreenConfigurationContext<SOInvoiceEntry, ARInvoice>());
        }

        protected virtual void Configure(WorkflowContext<SOInvoiceEntry, ARInvoice> context)
        {
            context.UpdateScreenConfigurationFor(screen =>
            {
                return screen.WithActions(actions =>
                {
                    actions.Add<SOInvoiceEntry_Extension>(e => e.printGUIInvoice,
                                                          a => a.WithCategory((PredefinedCategory)FolderType.ReportsFolder).PlaceAfter(s => s.printInvoice));
                });
            });
        }
        #endregion

        #region Actions
        public PXAction<ARInvoice> printGUIInvoice;
        [PXButton()]
        [PXUIField(DisplayName = "GUI Invoice", MapEnableRights = PXCacheRights.Select)]
        protected virtual IEnumerable PrintGUIInvoice(PXAdapter adapter)
        {
            if (Base.Document.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    [nameof(ARRegister.DocType)] = Base.Document.Current.DocType,
                    [nameof(ARRegister.RefNbr)] = Base.Document.Current.RefNbr
                };

                throw new PXReportRequiredException(parameters, GUIReportID, GUIReportID);
            }

            return adapter.Get();
        }
        #endregion

        #region Event Handlers
        protected void _(Events.RowSelected<SOInvoice> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            bool activateGUI = TWNGUIValidation.ActivateTWGUI(e.Cache.Graph);

            Base.report.SetVisible(nameof(PrintGUIInvoice), activateGUI);

            printGUIInvoice.SetEnabled(activateGUI && !string.IsNullOrEmpty(Base.Document.Current.GetExtension<ARRegisterExt>()?.UsrGUINbr));
        }
        #endregion
    }
}