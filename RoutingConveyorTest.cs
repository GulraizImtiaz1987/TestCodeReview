using ETS.Core.Api.Models.Data;
using NUnit.Framework;

namespace ConveyorProcessLogicTest
{
  internal class RoutingConveyorTest : TestMethod
  {
    const string server = "192.168.22.101";
    const string database = "RW_WCS_CORE";
    const string login = "edbApp";
    const string password = "Sqlapp!23";
    const int systemID = 146; // CONV_010200 (Turntable)
    const int defaultTblProductRoutingAssignSystemID = 169; // CONV_503 (Buffer at Docks)
    DbItem PalletItem;
    DbItem ProductItem;
    int PLC_ItemID;
    int JobID;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
      Init(systemID, server, database, login, password);
      SetSystemCustomPropertyValue(systemID, "CONV.OPERATIONSMODE", 1);
      SetSystemCustomPropertyValue(systemID, "DEFAULTROUTE", 2);
      SetProductCustomPropertyValue(-1, 823, "DESTINATIONLOCATIONID");
    }

    [SetUp]
    public void Setup()
    {
      ClearDB();
    }


    #region Good Flows

    [Test]
    public void RoutingConveryorGoodFlow_0_HeadingToTheDocks()
    {//Product Final Destination is set to the nextLinkedSystem

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, nextLinkedSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RoutingConveryorGoodFlow_3_HeadingToTheDocks()
    {//Product Final Destination is set to the nextLinkedSystem

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);
      
      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, nextLinkedSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RoutingConveryorGoodFlow_0_HeadingToTheAlt()
    {//Product Final Destination is set to the the Alt Destination System

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, alternateDestinationSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, alternateDestinationSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, alternateDestinationSystemID);

        CheckSetDestination(altConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RoutingConveryorGoodFlow_3_HeadingToTheAlt()
    {//Product Final Destination is set to the the Alt Destination System

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, alternateDestinationSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, alternateDestinationSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, alternateDestinationSystemID);

        CheckSetDestination(altConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RoutingConveryorGoodFlow_0_HeadingToTheEject()
    {//Product Final Destination is set to the Eject System ID

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, ejectDestinationSystemID); ;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, ejectDestinationSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, ejectDestinationSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RoutingConveryorGoodFlow_3_HeadingToTheEject()
    {//Product Final Destination is set to the Eject System ID

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, ejectDestinationSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, ejectDestinationSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, ejectDestinationSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RoutingConveryorGoodFlow_0_NoFinalDestination_UseProductType()
    { //Final Destination not set (-1)
      //Operation Mode Custom Property = 0 (Product Type)

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 3805;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      SetSystemCustomPropertyValue(systemID, "CONV.OPERATIONSMODE", 0);
      SetProductCustomPropertyValue(defaultTblProductRoutingAssignSystemID, ProductItem.ProductID, "DESTINATIONLOCATIONID");
      Init(systemID, server, database, login, password);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, defaultTblProductRoutingAssignSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, defaultTblProductRoutingAssignSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      //Revert the changes
      SetSystemCustomPropertyValue(systemID, "CONV.OPERATIONSMODE", 1);
      SetProductCustomPropertyValue(-1, ProductItem.ProductID, "DESTINATIONLOCATIONID");
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void RoutingConveryorGoodFlow_3_NoFinalDestination_UseProductType()
    { //Final Destination not set (-1)
      //Operation Mode Custom Property = 0 (Product Type)

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 3805;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      SetSystemCustomPropertyValue(systemID, "CONV.OPERATIONSMODE", 0);
      SetProductCustomPropertyValue(defaultTblProductRoutingAssignSystemID, ProductItem.ProductID, "DESTINATIONLOCATIONID");
      Init(systemID, server, database, login, password);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, defaultTblProductRoutingAssignSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, defaultTblProductRoutingAssignSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      //Revert the changes
      SetSystemCustomPropertyValue(systemID, "CONV.OPERATIONSMODE", 1);
      SetProductCustomPropertyValue(-1, ProductItem.ProductID, "DESTINATIONLOCATIONID");
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void RoutingConveryorGoodFlow_0_NoFinalDestination_UseProductTypeNoProductDestination()
    { //Final Destination not set (-1)
      //Operation Mode Custom Property = 0 (Product Type)
      //No destinaton set for product

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 3805;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      SetSystemCustomPropertyValue(systemID, "CONV.OPERATIONSMODE", 0);
      Init(systemID, server, database, login, password);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, ejectDestinationSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, ejectDestinationSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      //Revert the changes
      SetSystemCustomPropertyValue(systemID, "CONV.OPERATIONSMODE", 1);
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void RoutingConveryorGoodFlow_3_NoFinalDestination_UseProductTypeNoProductDestination()
    { //Final Destination not set (-1)
      //Operation Mode Custom Property = 0 (Product Type)
      //No destinaton set for product

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 3805;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      SetSystemCustomPropertyValue(systemID, "CONV.OPERATIONSMODE", 0);
      Init(systemID, server, database, login, password);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, ejectDestinationSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, ejectDestinationSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      //Revert the changes
      SetSystemCustomPropertyValue(systemID, "CONV.OPERATIONSMODE", 1);
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void RoutingConveryorGoodFlow_0_NoFinalDestination_UseProductList()
    { //Final Destination not set (-1)
      //Operation mode Custom Property = 1
      //Get final destination from tblProductRouting

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      InsertMaterialTestDataIntoTblProductRouting(ProductItem.ProductID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, defaultTblProductRoutingAssignSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, defaultTblProductRoutingAssignSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RoutingConveryorGoodFlow_3_NoFinalDestination_UseProductList()
    { //Final Destination not set (-1)
      //Operation mode Custom Property = 1
      //Get final destination from tblProductRouting

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      InsertMaterialTestDataIntoTblProductRouting(ProductItem.ProductID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, defaultTblProductRoutingAssignSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, defaultTblProductRoutingAssignSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RoutingConveryorGoodFlow_0_NoFinalDestination_UseProductList_DefaultRouteNormal()
    { //Final Destination not set (-1)
      //Operation mode Custom Property = 1
      //Default Route custom property = 2 (normal route)

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, nextLinkedSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RoutingConveryorGoodFlow_3_NoFinalDestination_UseProductList_DefaultRouteNormal()
    { //Final Destination not set (-1)
      //Operation mode Custom Property = 1
      //Default Route custom property = 2 (normal route)

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, nextLinkedSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RoutingConveryorGoodFlow_0_NoFinalDestination_UseProductList_DefaultRouteAlt()
    { //Final Destination not set (-1)
      //Operation mode Custom Property = 1
      //Default Route custom property = 0 (alt route)

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      SetSystemCustomPropertyValue(systemID, "DEFAULTROUTE", 0);
      Init(systemID, server, database, login, password);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, alternateDestinationSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, alternateDestinationSystemID);

        CheckSetDestination(altConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      // Revert the changes
      SetSystemCustomPropertyValue(systemID, "DEFAULTROUTE", 2);
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void RoutingConveryorGoodFlow_3_NoFinalDestination_UseProductList_DefaultRouteAlt()
    { //Final Destination not set (-1)
      //Operation mode Custom Property = 1
      //Default Route custom property = 0 (alt route)

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      SetSystemCustomPropertyValue(systemID, "DEFAULTROUTE", 0);
      Init(systemID, server, database, login, password);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, alternateDestinationSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, alternateDestinationSystemID);

        CheckSetDestination(altConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      // Revert the changes
      SetSystemCustomPropertyValue(systemID, "DEFAULTROUTE", 2);
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void RoutingConveryorGoodFlow_0_NoFinalDestination_UseProductList_DefaultRouteEject()
    { //Final Destination not set (-1)
      //Operation mode Custom Property = 1
      //Default Route custom property = 1 (eject route)

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      SetSystemCustomPropertyValue(systemID, "DEFAULTROUTE", 1);
      Init(systemID, server, database, login, password);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, ejectDestinationSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, ejectDestinationSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      // Revert the changes
      SetSystemCustomPropertyValue(systemID, "DEFAULTROUTE", 2);
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void RoutingConveryorGoodFlow_3_NoFinalDestination_UseProductList_DefaultRouteEject()
    { //Final Destination not set (-1)
      //Operation mode Custom Property = 1
      //Default Route custom property = 1 (eject route)

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      SetSystemCustomPropertyValue(systemID, "DEFAULTROUTE", 1);
      Init(systemID, server, database, login, password);;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, ejectDestinationSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, ejectDestinationSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      // Revert the changes
      SetSystemCustomPropertyValue(systemID, "DEFAULTROUTE", 2);
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void RoutingConveryorGoodFlow_1_EndOfTracking()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 1);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RoutingConveryorGoodFlow_2_AtDestination()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 2);

      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RoutingConveryorGoodFlow_4_TrackingLost()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 4);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, -1, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, -1, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    #endregion Good Flows

    #region Bad Flows

    [Test]
    public void RoutingConveryorBadFlow_0_InvalidPLCItemID()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 0;
      JobID = 15478;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RoutingConveryorBadFlow_0_NoItemsLinkedToPLC_ItemID()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RoutingConveryorBadFlow_3_NoItemsLinkedToPLC_ItemID()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RoutingConveryorBadFlow_0_NoProduct()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, ejectDestinationSystemID);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RoutingConveryorBadFlow_3_NoProduct()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, ejectDestinationSystemID);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RoutingConveryorBadFlow_0_NoPallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      ProductItem = CreateProduct(1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, PLCReference, ejectDestinationSystemID);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RoutingConveryorBadFlow_3_NoPallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      ProductItem = CreateProduct(1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, PLCReference, ejectDestinationSystemID);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    #endregion Bad Flows
  }
}
