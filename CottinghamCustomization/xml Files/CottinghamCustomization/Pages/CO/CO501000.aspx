<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CO501000.aspx.cs" Inherits="Page_CO501000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="CottinghamCustomization.COImportWMSOrdersProc" PrimaryView="Filter">
		<CallbackCommands>

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" Height="100px" AllowAutoHide="false">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartRow="True"></px:PXLayoutRule>
		</Template>
	</px:PXFormView>
	<px:PXTab runat="server" ID="CstPXTab1">
		<Items>
			<px:PXTabItem Text="TabItem1">
				<Template>
					<px:PXGrid SkinID="DetailsInTab" runat="server" ID="CstPXGrid2" Width="100%" DataSourceID="ds" SyncPosition="True">
						<Levels>
							<px:PXGridLevel DataMember="Receipt" ></px:PXGridLevel></Levels>
						<Mode AllowUpload="True" ></Mode>
						<AutoSize Enabled="True" /></px:PXGrid></Template></px:PXTabItem>
			<px:PXTabItem>
				<Template>
					<px:PXGrid runat="server" ID="CstPXGrid3" SyncPosition="True" DataSourceID="ds" Width="100%" SkinID="DetailsInTab">
						<Levels>
							<px:PXGridLevel DataMember="Shipment" ></px:PXGridLevel></Levels>
						<Mode AllowUpload="True" /></px:PXGrid></Template></px:PXTabItem></Items></px:PXTab></asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server"></asp:Content>