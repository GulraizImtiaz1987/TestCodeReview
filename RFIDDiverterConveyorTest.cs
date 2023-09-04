using ETS.Core.Api.Models.Data;
using NUnit.Framework;

namespace ConveyorProcessLogicTest
{
  internal class RFIDDiverterConveyorTest : TestMethod
  {
    const string server = "192.168.22.101";
    const string database = "RW_WCS_CORE";
    const string login = "edbApp";
    const string password = "Sqlapp!23";
    const int systemID = 336; // Test RFID Diverter Conveyor
    DbItem PalletItem;
    DbItem ProductItem;
    int PLC_ItemID;
    int JobID;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
      Init(systemID, server, database, login, password);
    }

    [SetUp]
    public void Setup()
    {
      ClearDB();
    }

    #region Good Flows

    [Test]
    public void RFIDDiverterConveyorGoodFlow_1()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 4161;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 1);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = string.Empty;
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, warehouseLocationID, PLCItemLineReference);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, warehouseLocationID, PLCItemLineReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDDiverterConveyorGoodFlow_2()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 2);

      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDDiverterConveyorGoodFlow_4()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 4161;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 4);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = string.Empty;
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, -1, PLCItemLineReference);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, -1, -1, PLCItemLineReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDDiverterConveyorGoodFlow_0_SemiFinishedNewGRAI()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 0);
      SetSystemCustomPropertyValue(system.ID, "CP.SEMIFINISHEDROUTINGPATH", 1);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, GRAI, palletItemDefinitionID, system.LocationID, PLCItemLineReference, alternateDestinationSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, alternateDestinationSystemID);

        CheckSetDestination(altConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDDiverterConveyorGoodFlow_3_SemiFinishedNewGRAI()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 0);
      SetSystemCustomPropertyValue(system.ID, "CP.SEMIFINISHEDROUTINGPATH", 1);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, GRAI, palletItemDefinitionID, system.LocationID, PLCItemLineReference, alternateDestinationSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, alternateDestinationSystemID);

        CheckSetDestination(altConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDDiverterConveyorGoodFlow_0_SemiFinishedExistingGRAI()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 0);
      SetSystemCustomPropertyValue(system.ID, "CP.SEMIFINISHEDROUTINGPATH", 1);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";

      DbItem ExistingPalletItem = CreatePallet(GRAI, 1, JobID, 5);
      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(ExistingPalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, ExistingPalletItem.ID, system.LocationID, PLCItemLineReference);
        CheckPalletIsRemoved(PalletItem.ID);

        CheckSetDestination(altConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDDiverterConveyorGoodFlow_3_SemiFinishedExistingGRAI()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 0);
      SetSystemCustomPropertyValue(system.ID, "CP.SEMIFINISHEDROUTINGPATH", 1);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";

      DbItem ExistingPalletItem = CreatePallet(GRAI, 1, JobID, 5);
      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(ExistingPalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, ExistingPalletItem.ID, system.LocationID, PLCItemLineReference);
        CheckPalletIsRemoved(PalletItem.ID);

        CheckSetDestination(altConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDDiverterConveyorGoodFlow_0_SemiFinished_ProductNoRFIDRequired()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 0);
      SetSystemCustomPropertyValue(system.ID, "CP.SEMIFINISHEDROUTINGPATH", 1);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 3679;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, nextLinkedSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDDiverterConveyorGoodFlow_3_SemiFinished_ProductNoRFIDRequired()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 0);
      SetSystemCustomPropertyValue(system.ID, "CP.SEMIFINISHEDROUTINGPATH", 1);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 3679;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, nextLinkedSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDDiverterConveyorGoodFlow_0_ProductTypeRouting()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 1);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4290;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      InsertTestDataIntoTblProductRouting(alternateDestinationSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, alternateDestinationSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, alternateDestinationSystemID);

        CheckSetDestination(altConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDDiverterConveyorGoodFlow_3_ProductTypeRouting()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 1);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4290;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      InsertTestDataIntoTblProductRouting(alternateDestinationSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, alternateDestinationSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, alternateDestinationSystemID);

        CheckSetDestination(altConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDDiverterConveyorGoodFlow_0_ProductListRouting_ProductOnList()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 2);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, alternateDestinationSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, alternateDestinationSystemID);

        CheckSetDestination(altConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDDiverterConveyorGoodFlow_3_ProductListRouting_ProductOnList()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 2);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, alternateDestinationSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, alternateDestinationSystemID);

        CheckSetDestination(altConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDDiverterConveyorGoodFlow_0_ProductListRouting_ProductNotOnList()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 2);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 3679;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, nextLinkedSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDDiverterConveyorGoodFlow_3_ProductListRouting_ProductNotOnList()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 2);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 3679;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, nextLinkedSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDDiverterConveyorGoodFlow_0_ProductSetupRouting_UseAltRouteAsDefault()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 3);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      SetProductCustomPropertyValue(1, ProductItem.ProductID, "CP.USEALTERNATIVEROUTEASDEFAULT");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, alternateDestinationSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, alternateDestinationSystemID);

        CheckSetDestination(altConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDDiverterConveyorGoodFlow_3_ProductSetupRouting_UseAltRouteAsDefault()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 3);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      SetProductCustomPropertyValue(1, ProductItem.ProductID, "CP.USEALTERNATIVEROUTEASDEFAULT");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, alternateDestinationSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, alternateDestinationSystemID);

        CheckSetDestination(altConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDDiverterConveyorGoodFlow_0_ProductSetupRouting_DontUseAltRouteAsDefault()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 3);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 3679;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      SetProductCustomPropertyValue(0, ProductItem.ProductID, "CP.USEALTERNATIVEROUTEASDEFAULT");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, nextLinkedSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDDiverterConveyorGoodFlow_3_ProductSetupRouting_DontUseAltRouteAsDefault()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 3);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 3679;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      SetProductCustomPropertyValue(0, ProductItem.ProductID, "CP.USEALTERNATIVEROUTEASDEFAULT");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, nextLinkedSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    #endregion Good Flows

    #region Bad Flows

    [Test]
    public void RFIDDiverterConveyorBadFlow_InvalidPLCItemID()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 0;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDDiverterConveyorBadFlow_0_SemiFinished_NoPalletItem()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDDiverterConveyorBadFlow_3_SemiFinished_NoPalletItem()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDDiverterConveyorBadFlow_0_SemiFinished_NoProductItem()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 4161;
      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, ejectDestinationSystemID);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDDiverterConveyorBadFlow_3_SemiFinished_NoProductItem()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 4161;
      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, ejectDestinationSystemID);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDDiverterConveyorBadFlow_0_SemiFinished_RFIDTimeout()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 0);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 3, false);

      // Wait for timeout
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, ejectDestinationSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, ejectDestinationSystemID);

        CheckSetDestination(ejectConv_PositionID);

        CheckEventCreatedByDefinitionID(RFIDTimeoutEventDefinitionID);
        CheckUserTask(ManualRFIDUserTaskDefinitionID, AtPositionTaskID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDDiverterConveyorBadFlow_3_SemiFinished_RFIDTimeout()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 0);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 3, false);

      // Wait for timeout
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, ejectDestinationSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, ejectDestinationSystemID);

        CheckSetDestination(ejectConv_PositionID);

        CheckEventCreatedByDefinitionID(RFIDTimeoutEventDefinitionID);
        CheckUserTask(ManualRFIDUserTaskDefinitionID, AtPositionTaskID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDDiverterConveyorBadFlow_0_SemiFinished_RFIDNoRead()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 0);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "NoRead";

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, ejectDestinationSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, ejectDestinationSystemID);

        CheckSetDestination(ejectConv_PositionID);

        CheckEventCreatedByDefinitionID(RFIDNoReadEventDefinitionID);
        CheckUserTask(ManualRFIDUserTaskDefinitionID, AtPositionTaskID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDDiverterConveyorBadFlow_3_SemiFinished_RFIDNoRead()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 0);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "NoRead";

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, ejectDestinationSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, ejectDestinationSystemID);

        CheckSetDestination(ejectConv_PositionID);

        CheckEventCreatedByDefinitionID(RFIDNoReadEventDefinitionID);
        CheckUserTask(ManualRFIDUserTaskDefinitionID, AtPositionTaskID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDDiverterConveyorBadFlow_0_ProductTypeRouting_NoProductRouting()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 1);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4290;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, ejectDestinationSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, ejectDestinationSystemID);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDDiverterConveyorBadFlow_3_ProductTypeRouting_NoProductRouting()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.OPERATIONSMODEDIVERTERRFID", 1);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4290;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, ejectDestinationSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, ejectDestinationSystemID);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    #endregion Bad Flows
  }
}
