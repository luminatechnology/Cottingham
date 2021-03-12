<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="GL411000.aspx.cs" Inherits="Page_GL411000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="CottinghamCustomization.COProdContribInq" PrimaryView="Filter" >
		<CallbackCommands>

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" AllowAutoHide="false">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartRow="True"></px:PXLayoutRule>
			<px:PXSegmentMask runat="server" ID="CstPXSegmentMask1" DataField="BranchID" ></px:PXSegmentMask>
			<px:PXSelector runat="server" ID="CstPXSelector3" DataField="LedgerID" ></px:PXSelector>
			<px:PXSelector runat="server" ID="CstPXSelector5" DataField="UsrFinPeriodID" CommitChanges="True" ></px:PXSelector>
			<px:PXSegmentMask CommitChanges="True" runat="server" ID="CstPXSegmentMask4" DataField="SubIDFilter" ></px:PXSegmentMask></Template>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" ></AutoSize>
	</px:PXFormView>
</asp:Content>