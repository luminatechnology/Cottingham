<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CO401000.aspx.cs" Inherits="Page_CO401000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="CottinghamCustomization.COShowroomInventDetailsInq" PrimaryView="Filter">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" Height="50px" AllowAutoHide="false">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartRow="True"></px:PXLayoutRule>
			<px:PXSelector runat="server" ID="CstPXSelector1" DataField="TenantID" CommitChanges="true" ></px:PXSelector></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Details" AllowAutoHide="false">
		<Levels>
			<px:PXGridLevel DataMember="InventDetails">
			    <Columns>
				<px:PXGridColumn DataField="InventoryID" Width="100" />
				<px:PXGridColumn DataField="InventDescr" Width="250" />
				<px:PXGridColumn DataField="ItemClassCD" Width="80" />
				<px:PXGridColumn DataField="PostClassID" Width="80" />
				<px:PXGridColumn DataField="PostClassDescr" Width="180" />
				<px:PXGridColumn DataField="SiteCD" Width="80" />
				<px:PXGridColumn DataField="LocationCD" Width="80" />
				<px:PXGridColumn DataField="SalesPrice" Width="100" />
				<px:PXGridColumn DataField="QtyAvail" Width="120" /></Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar PagerVisible="Bottom" >
			<PagerSettings Mode="NumericCompact" />
		</ActionBar>
	</px:PXGrid>
</asp:Content>