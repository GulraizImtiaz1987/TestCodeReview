using ETS.Core.Api.Models.Data;
using ETS.Core.Enums;
using ETS.Core.Extensions;
using NUnit.Framework;

namespace ConveyorProcessLogicTest
{
  public class UnitConsumptionConveyorTest : TestMethod
  {
    const string server = "192.168.22.101";
    const string database = "RW_WCS_CORE";
    const string login = "edbApp";
    const string password = "Sqlapp!23";
    const int systemIDWithRFID = 305;
    const int systemIDWithoutRFID = 331;

    DbItem PalletItem;
    DbItem ProductItem;
    DbJobSystemActual Jsa;
    int PLC_ItemID;
    int JobID;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
      Init(systemIDWithRFID, server, database, login, password);
    }

    [SetUp]
    public void Setup()
    {
      ClearDB();
    }

    #region Good Flows - Conveyor with RFID reader

    [Test]
    public void UnitConsumptionConveyorGoodFlow_1()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

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
    public void UnitConsumptionConveyorGoodFlow_4()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

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

    [Test]
    public void UnitConsumptionConveyorGoodFlow_0_RFIDGRAI_ConsumePallet()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";
      int quantity = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 3, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckInitialQuantity(ProductItem.ID, quantity);

      if (RunTask(AtPositionTaskID))
      {
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);

        CheckMaterialConsumption(ProductItem, quantity);

        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, quantity, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_3_RFIDGRAI_ConsumePallet()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";
      int quantity = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 3, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckInitialQuantity(ProductItem.ID, quantity);

      if (RunTask(AtPositionTaskID))
      {
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);

        CheckMaterialConsumption(ProductItem, quantity);

        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, quantity, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_0_RFIDGRAI_ConsumeUnitsAll()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";
      int quantity = 4;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 4, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckInitialQuantity(ProductItem.ID, quantity);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 3);
      CheckMaterialConsumption(ProductItem, 1);
      CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.PARTIAL);

      SendConsumeUnit(PLC_ItemID, 2, 1); // Consume 2 units
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 1);
      CheckMaterialConsumption(ProductItem, 2);
      CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 2, PalletConsumptionType.PARTIAL);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      if (RunTask(AtPositionTaskID))
      {
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);

        CheckMaterialConsumption(ProductItem, 1);

        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_3_RFIDGRAI_ConsumeUnitsAll()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";
      int quantity = 4;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 4, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckInitialQuantity(ProductItem.ID, quantity);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 3);
      CheckMaterialConsumption(ProductItem, 1);
      CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.PARTIAL);

      SendConsumeUnit(PLC_ItemID, 2, 1); // Consume 2 units
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 1);
      CheckMaterialConsumption(ProductItem, 2);
      CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 2, PalletConsumptionType.PARTIAL);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      if (RunTask(AtPositionTaskID))
      {
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);

        CheckMaterialConsumption(ProductItem, 1);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_0_RFIDGRAI_ConsumeUnitsPartially()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";
      int quantity = 10;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "0");

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 4, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckInitialQuantity(ProductItem.ID, quantity);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 9);
      CheckMaterialConsumption(ProductItem, 1);
      CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.PARTIAL);

      SendConsumeUnit(PLC_ItemID, 4, 1); // Consume 4 units
      RunTask(AtPositionTaskID, 1, false);

      int AtPositionTaskID2 = SendAtPosition(PLC_ItemID, 1); // Remove half full pallet from conveyor
      if (RunTask(AtPositionTaskID))
      {
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);

        CheckProductQuantity(ProductItem.ID, 5);
        CheckMaterialConsumption(ProductItem, 4);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 4, PalletConsumptionType.PARTIAL);

        CheckMessageUserStateByMessageID(AtPositionTaskID, TaskUserState.COMPLETE);
        CheckMessagePassFailByMessageID(AtPositionTaskID, TaskPassFail.PASS);

        if (RunTask(AtPositionTaskID2))
        {
          CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, string.Empty);
          CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, string.Empty);

          CheckMessageUserStateByMessageID(AtPositionTaskID2, TaskUserState.COMPLETE);
          CheckMessagePassFailByMessageID(AtPositionTaskID2, TaskPassFail.PASS);
        }
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_3_RFIDGRAI_ConsumeUnitsPartially()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";
      int quantity = 10;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "0");

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 4, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckInitialQuantity(ProductItem.ID, quantity);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 9);
      CheckMaterialConsumption(ProductItem, 1);
      CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.PARTIAL);

      SendConsumeUnit(PLC_ItemID, 4, 1); // Consume 4 units
      RunTask(AtPositionTaskID, 1, false);

      int AtPositionTaskID2 = SendAtPosition(PLC_ItemID, 1); // Remove half full pallet from conveyor
      if (RunTask(AtPositionTaskID))
      {
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);

        CheckProductQuantity(ProductItem.ID, 5);
        CheckMaterialConsumption(ProductItem, 4);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 4, PalletConsumptionType.PARTIAL);

        CheckMessageUserStateByMessageID(AtPositionTaskID, TaskUserState.COMPLETE);
        CheckMessagePassFailByMessageID(AtPositionTaskID, TaskPassFail.PASS);

        if (RunTask(AtPositionTaskID2))
        {
          CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, string.Empty);
          CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, string.Empty);

          CheckMessageUserStateByMessageID(AtPositionTaskID2, TaskUserState.COMPLETE);
          CheckMessagePassFailByMessageID(AtPositionTaskID2, TaskPassFail.PASS);
        }
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_0_RFIDSSCC_ConsumePallet()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string SSCC = "05715333100738868";
      int quantity = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      ProductItem = SetProductItemSSCC(ProductItem, SSCC);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, SSCC);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 3, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckInitialQuantity(ProductItem.ID, quantity);

      if (RunTask(AtPositionTaskID))
      {
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, quantity);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_3_RFIDSSCC_ConsumePallet()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string SSCC = "05715333100738868";
      int quantity = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      ProductItem = SetProductItemSSCC(ProductItem, SSCC);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, SSCC);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 3, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckInitialQuantity(ProductItem.ID, quantity);

      if (RunTask(AtPositionTaskID))
      {
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, quantity);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_0_RFIDSSCC_ConsumeUnitsAll()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string SSCC = "05715333100738868";
      int quantity = 4;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      ProductItem = SetProductItemSSCC(ProductItem, SSCC);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, SSCC);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 4, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 3);
      CheckMaterialConsumption(ProductItem, 1);
      CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.PARTIAL);

      SendConsumeUnit(PLC_ItemID, 2, 1); // Consume 2 units
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 1);
      CheckMaterialConsumption(ProductItem, 2);
      CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 2, PalletConsumptionType.PARTIAL);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, 1);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_3_RFIDSSCC_ConsumeUnitsAll()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string SSCC = "05715333100738868";
      int quantity = 4;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      ProductItem = SetProductItemSSCC(ProductItem, SSCC);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, SSCC);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 4, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 3);
      CheckMaterialConsumption(ProductItem, 1);
      CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.PARTIAL);

      SendConsumeUnit(PLC_ItemID, 2, 1); // Consume 2 units
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 1);
      CheckMaterialConsumption(ProductItem, 2);
      CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 2, PalletConsumptionType.PARTIAL);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, 1);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_0_RFIDSSCC_ConsumeUnitsPartially()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string SSCC = "05715333100738868";
      int quantity = 10;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      ProductItem = SetProductItemSSCC(ProductItem, SSCC);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, SSCC);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 4, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 9);
      CheckMaterialConsumption(ProductItem, 1);
      CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.PARTIAL);

      SendConsumeUnit(PLC_ItemID, 4, 1); // Consume 4 units
      RunTask(AtPositionTaskID, 1, false);

      int AtPositionTaskID2 = SendAtPosition(PLC_ItemID, 1); // Remove half full pallet from conveyor
      if (RunTask(AtPositionTaskID))
      {
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);

        CheckProductQuantity(ProductItem.ID, 5);
        CheckMaterialConsumption(ProductItem, 4);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 4, PalletConsumptionType.PARTIAL);

        CheckMessageUserStateByMessageID(AtPositionTaskID, TaskUserState.COMPLETE);
        CheckMessagePassFailByMessageID(AtPositionTaskID, TaskPassFail.PASS);

        if (RunTask(AtPositionTaskID2))
        {
          CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, string.Empty);
          CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, string.Empty);

          CheckMessageUserStateByMessageID(AtPositionTaskID2, TaskUserState.COMPLETE);
          CheckMessagePassFailByMessageID(AtPositionTaskID2, TaskPassFail.PASS);
        }
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_3_RFIDSSCC_ConsumeUnitsPartially()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string SSCC = "05715333100738868";
      int quantity = 10;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      ProductItem = SetProductItemSSCC(ProductItem, SSCC);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, SSCC);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 4, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 9);
      CheckMaterialConsumption(ProductItem, 1);
      CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.PARTIAL);

      SendConsumeUnit(PLC_ItemID, 4, 1); // Consume 4 units
      RunTask(AtPositionTaskID, 1, false);

      int AtPositionTaskID2 = SendAtPosition(PLC_ItemID, 1); // Remove half full pallet from conveyor
      if (RunTask(AtPositionTaskID))
      {
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);

        CheckProductQuantity(ProductItem.ID, 5);
        CheckMaterialConsumption(ProductItem, 4);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 4, PalletConsumptionType.PARTIAL);

        CheckMessageUserStateByMessageID(AtPositionTaskID, TaskUserState.COMPLETE);
        CheckMessagePassFailByMessageID(AtPositionTaskID, TaskPassFail.PASS);

        if (RunTask(AtPositionTaskID2))
        {
          CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, string.Empty);
          CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, string.Empty);

          CheckMessageUserStateByMessageID(AtPositionTaskID2, TaskUserState.COMPLETE);
          CheckMessagePassFailByMessageID(AtPositionTaskID2, TaskPassFail.PASS);
        }
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_0_RFIDTimeout_ManualTaskPass_ConsumePallet()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";
      int quantity = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CREATETASKONREADFAILURE", "1");
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(AtPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualRFIDUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      RunTask(AtPositionTaskID, 1, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckInitialQuantity(ProductItem.ID, quantity);

      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, quantity);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        int UserTaskID = CheckUserTask(ManualRFIDUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckEventCreatedByDefinitionID(RFIDTimeoutEventDefinitionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_3_RFIDTimeout_ManualTaskPass_ConsumePallet()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";
      int quantity = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CREATETASKONREADFAILURE", "1");
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(AtPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualRFIDUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      RunTask(AtPositionTaskID, 1, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckInitialQuantity(ProductItem.ID, quantity);

      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, quantity);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        int UserTaskID = CheckUserTask(ManualRFIDUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);
        
        CheckEventCreatedByDefinitionID(RFIDTimeoutEventDefinitionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_0_RFIDTimeout_ManualTaskPass_ConsumeUnitsAll()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";
      int quantity = 4;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CREATETASKONREADFAILURE", "1");
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(AtPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualRFIDUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      RunTask(AtPositionTaskID, 2, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 3);
      CheckMaterialConsumption(ProductItem, 1);
      CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.PARTIAL);

      SendConsumeUnit(PLC_ItemID, 2, 1); // Consume 2 units
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 1);
      CheckMaterialConsumption(ProductItem, 2);
      CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 2, PalletConsumptionType.PARTIAL);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, 1);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        int UserTaskID = CheckUserTask(ManualRFIDUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckEventCreatedByDefinitionID(RFIDTimeoutEventDefinitionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_3_RFIDTimeout_ManualTaskPass_ConsumeUnitsAll()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";
      int quantity = 4;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CREATETASKONREADFAILURE", "1");
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(AtPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualRFIDUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      RunTask(AtPositionTaskID, 2, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 3);
      CheckMaterialConsumption(ProductItem, 1);
      CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.PARTIAL);

      SendConsumeUnit(PLC_ItemID, 2, 1); // Consume 2 units
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 1);
      CheckMaterialConsumption(ProductItem, 2);
      CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 2, PalletConsumptionType.PARTIAL);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, 1);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        int UserTaskID = CheckUserTask(ManualRFIDUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckEventCreatedByDefinitionID(RFIDTimeoutEventDefinitionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_0_RFIDTimeout_ManualTaskPass_ConsumeUnitsPartially()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";
      int quantity = 10;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CREATETASKONREADFAILURE", "1");
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(AtPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualRFIDUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      RunTask(AtPositionTaskID, 2, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 9);
      CheckMaterialConsumption(ProductItem, 1);
      CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.PARTIAL);

      SendConsumeUnit(PLC_ItemID, 4, 1); // Consume 4 units
      RunTask(AtPositionTaskID, 1, false);

      int AtPositionTaskID2 = SendAtPosition(PLC_ItemID, 1); // Remove half full pallet from conveyor
      if (RunTask(AtPositionTaskID))
      {
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);

        CheckProductQuantity(ProductItem.ID, 5);
        CheckMaterialConsumption(ProductItem, 4);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 4, PalletConsumptionType.PARTIAL);

        CheckMessageUserStateByMessageID(AtPositionTaskID, TaskUserState.COMPLETE);
        CheckMessagePassFailByMessageID(AtPositionTaskID, TaskPassFail.PASS);

        if (RunTask(AtPositionTaskID2))
        {
          CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, string.Empty);
          CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, string.Empty);

          int UserTaskID = CheckUserTask(ManualRFIDUserTaskDefinitionID, AtPositionTaskID);
          CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
          CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

          CheckEventCreatedByDefinitionID(RFIDTimeoutEventDefinitionID);

          CheckMessageUserStateByMessageID(AtPositionTaskID2, TaskUserState.COMPLETE);
          CheckMessagePassFailByMessageID(AtPositionTaskID2, TaskPassFail.PASS);
        }
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_3_RFIDTimeout_ManualTaskPass_ConsumeUnitsPartially()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";
      int quantity = 10;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CREATETASKONREADFAILURE", "1");
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(AtPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualRFIDUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      RunTask(AtPositionTaskID, 2, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 9);
      CheckMaterialConsumption(ProductItem, 1);
      CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.PARTIAL);

      SendConsumeUnit(PLC_ItemID, 4, 1); // Consume 4 units
      RunTask(AtPositionTaskID, 1, false);

      int AtPositionTaskID2 = SendAtPosition(PLC_ItemID, 1); // Remove half full pallet from conveyor
      if (RunTask(AtPositionTaskID))
      {
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);

        CheckProductQuantity(ProductItem.ID, 5);
        CheckMaterialConsumption(ProductItem, 4);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 4, PalletConsumptionType.PARTIAL);

        CheckMessageUserStateByMessageID(AtPositionTaskID, TaskUserState.COMPLETE);
        CheckMessagePassFailByMessageID(AtPositionTaskID, TaskPassFail.PASS);

        if (RunTask(AtPositionTaskID2))
        {
          CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, string.Empty);
          CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, string.Empty);

          int UserTaskID = CheckUserTask(ManualRFIDUserTaskDefinitionID, AtPositionTaskID);
          CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
          CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

          CheckEventCreatedByDefinitionID(RFIDTimeoutEventDefinitionID);

          CheckMessageUserStateByMessageID(AtPositionTaskID2, TaskUserState.COMPLETE);
          CheckMessagePassFailByMessageID(AtPositionTaskID2, TaskPassFail.PASS);
        }
      }
    }

    #endregion Good Flows - Conveyor with RFID reader

    #region Good Flows - Conveyor without RFID reader

    [Test]
    public void UnitConsumptionConveyorGoodFlow_0_NoRFID_ConsumePallet()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      int quantity = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      SetSystemCustomPropertyValue(systemIDWithoutRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckInitialQuantity(ProductItem.ID, quantity);

      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, quantity);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_3_NoRFID_ConsumePallet()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      int quantity = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      SetSystemCustomPropertyValue(systemIDWithoutRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckInitialQuantity(ProductItem.ID, quantity);

      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, quantity);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_0_NoRFID_ConsumeUnitsAll()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      int quantity = 4;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      SetSystemCustomPropertyValue(systemIDWithoutRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 3, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 3);
      CheckMaterialConsumption(ProductItem, 1);

      SendConsumeUnit(PLC_ItemID, 2, 1); // Consume 2 units
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 1);
      CheckMaterialConsumption(ProductItem, 2);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, 1);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 4, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_3_NoRFID_ConsumeUnitsAll()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      int quantity = 4;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      SetSystemCustomPropertyValue(systemIDWithoutRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 3, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 3);
      CheckMaterialConsumption(ProductItem, 1);

      SendConsumeUnit(PLC_ItemID, 2, 1); // Consume 2 units
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 1);
      CheckMaterialConsumption(ProductItem, 2);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, 1);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 4, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_0_NoRFID_ConsumeUnitsPartially()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      int quantity = 10;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      SetSystemCustomPropertyValue(systemIDWithoutRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 4, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 9);
      CheckMaterialConsumption(ProductItem, 1);

      SendConsumeUnit(PLC_ItemID, 4, 1); // Consume 4 units
      RunTask(AtPositionTaskID, 1, false);

      int AtPositionTaskID2 = SendAtPosition(PLC_ItemID, 1); // Remove half full pallet from conveyor
      if (RunTask(AtPositionTaskID))
      {
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
        CheckProductQuantity(ProductItem.ID, 5);
        CheckMaterialConsumption(ProductItem, 4);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 5, PalletConsumptionType.PARTIAL);

        CheckMessageUserStateByMessageID(AtPositionTaskID, TaskUserState.COMPLETE);
        CheckMessagePassFailByMessageID(AtPositionTaskID, TaskPassFail.PASS);

        if (RunTask(AtPositionTaskID2))
        {
          CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, string.Empty);
          CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, warehouseLocationID, string.Empty);
          CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
          CheckInitialQuantity(ProductItem.ID, quantity);

          CheckMessageUserStateByMessageID(AtPositionTaskID2, TaskUserState.COMPLETE);
          CheckMessagePassFailByMessageID(AtPositionTaskID2, TaskPassFail.PASS);
        }
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_3_NoRFID_ConsumeUnitsPartially()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      int quantity = 10;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);
      SetSystemCustomPropertyValue(systemIDWithoutRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 4, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 9);
      CheckMaterialConsumption(ProductItem, 1);

      SendConsumeUnit(PLC_ItemID, 4, 1); // Consume 4 units
      RunTask(AtPositionTaskID, 1, false);

      int AtPositionTaskID2 = SendAtPosition(PLC_ItemID, 1); // Remove half full pallet from conveyor
      if (RunTask(AtPositionTaskID))
      {
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
        CheckProductQuantity(ProductItem.ID, 5);
        CheckMaterialConsumption(ProductItem, 4);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 5, PalletConsumptionType.PARTIAL);

        CheckMessageUserStateByMessageID(AtPositionTaskID, TaskUserState.COMPLETE);
        CheckMessagePassFailByMessageID(AtPositionTaskID, TaskPassFail.PASS);

        if (RunTask(AtPositionTaskID2))
        {
          CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, string.Empty);
          CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, warehouseLocationID, string.Empty);
          CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
          CheckInitialQuantity(ProductItem.ID, quantity);

          CheckMessageUserStateByMessageID(AtPositionTaskID2, TaskUserState.COMPLETE);
          CheckMessagePassFailByMessageID(AtPositionTaskID2, TaskPassFail.PASS);
        }
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_0_NoRFID_NoPallet_ManualTaskPass_ConsumePallet()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      int quantity = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      SetSystemCustomPropertyValue(systemIDWithoutRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      PalletItem = CreatePallet("", system.LocationID, JobID, PLC_ItemID);
      ProductItem = CreateProduct(system.LocationID, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      RunTask(AtPositionTaskID, 1, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckInitialQuantity(ProductItem.ID, quantity);

      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, quantity);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_3_NoRFID_NoPallet_ManualTaskPass_ConsumePallet()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      int quantity = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      SetSystemCustomPropertyValue(systemIDWithoutRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      PalletItem = CreatePallet("", system.LocationID, JobID, PLC_ItemID);
      ProductItem = CreateProduct(system.LocationID, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      RunTask(AtPositionTaskID, 1, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckInitialQuantity(ProductItem.ID, quantity);

      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, quantity);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_0_NoRFID_NoPallet_ManualTaskPass_ConsumeUnitsAll()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      int quantity = 4;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      SetSystemCustomPropertyValue(systemIDWithoutRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      PalletItem = CreatePallet("", system.LocationID, JobID, PLC_ItemID);
      ProductItem = CreateProduct(system.LocationID, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      RunTask(AtPositionTaskID, 2, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 3);
      CheckMaterialConsumption(ProductItem, 1);

      SendConsumeUnit(PLC_ItemID, 2, 1); // Consume 2 units
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 1);
      CheckMaterialConsumption(ProductItem, 2);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, 1);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 4, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);;

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_3_NoRFID_NoPallet_ManualTaskPass_ConsumeUnitsAll()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      int quantity = 4;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      SetSystemCustomPropertyValue(systemIDWithoutRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      PalletItem = CreatePallet("", system.LocationID, JobID, PLC_ItemID);
      ProductItem = CreateProduct(system.LocationID, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      RunTask(AtPositionTaskID, 2, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 3);
      CheckMaterialConsumption(ProductItem, 1);

      SendConsumeUnit(PLC_ItemID, 2, 1); // Consume 2 units
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 1);
      CheckMaterialConsumption(ProductItem, 2);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, 1);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 4, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID); ;

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_0_NoRFID_NoPallet_ManualTaskPass_ConsumeUnitsPartially()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      int quantity = 10;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      SetSystemCustomPropertyValue(systemIDWithoutRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      PalletItem = CreatePallet("", system.LocationID, JobID, PLC_ItemID);
      ProductItem = CreateProduct(system.LocationID, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      RunTask(AtPositionTaskID, 2, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 9);
      CheckMaterialConsumption(ProductItem, 1);

      SendConsumeUnit(PLC_ItemID, 4, 1); // Consume 4 units
      RunTask(AtPositionTaskID, 1, false);

      int AtPositionTaskID2 = SendAtPosition(PLC_ItemID, 1); // Remove half full pallet from conveyor
      if (RunTask(AtPositionTaskID))
      {
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
        CheckProductQuantity(ProductItem.ID, 5);
        CheckMaterialConsumption(ProductItem, 4);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 5, PalletConsumptionType.PARTIAL);

        CheckMessageUserStateByMessageID(AtPositionTaskID, TaskUserState.COMPLETE);
        CheckMessagePassFailByMessageID(AtPositionTaskID, TaskPassFail.PASS);

        if (RunTask(AtPositionTaskID2))
        {
          CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, string.Empty);
          CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, string.Empty);

          CheckMessageUserStateByMessageID(AtPositionTaskID2, TaskUserState.COMPLETE);
          CheckMessagePassFailByMessageID(AtPositionTaskID2, TaskPassFail.PASS);
        }
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_3_NoRFID_NoPallet_ManualTaskPass_ConsumeUnitsPartially()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      int quantity = 10;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      SetSystemCustomPropertyValue(systemIDWithoutRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      PalletItem = CreatePallet("", system.LocationID, JobID, PLC_ItemID);
      ProductItem = CreateProduct(system.LocationID, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      RunTask(AtPositionTaskID, 2, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 9);
      CheckMaterialConsumption(ProductItem, 1);

      SendConsumeUnit(PLC_ItemID, 4, 1); // Consume 4 units
      RunTask(AtPositionTaskID, 1, false);

      int AtPositionTaskID2 = SendAtPosition(PLC_ItemID, 1); // Remove half full pallet from conveyor
      if (RunTask(AtPositionTaskID))
      {
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
        CheckProductQuantity(ProductItem.ID, 5);
        CheckMaterialConsumption(ProductItem, 4);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 5, PalletConsumptionType.PARTIAL);

        CheckMessageUserStateByMessageID(AtPositionTaskID, TaskUserState.COMPLETE);
        CheckMessagePassFailByMessageID(AtPositionTaskID, TaskPassFail.PASS);

        if (RunTask(AtPositionTaskID2))
        {
          CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, string.Empty);
          CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, string.Empty);

          CheckMessageUserStateByMessageID(AtPositionTaskID2, TaskUserState.COMPLETE);
          CheckMessagePassFailByMessageID(AtPositionTaskID2, TaskPassFail.PASS);
        }
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_0_NoRFID_NoProduct_ManualTaskPass_ConsumePallet()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      int quantity = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      SetSystemCustomPropertyValue(systemIDWithoutRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      ProductItem = CreateProduct(system.LocationID, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      RunTask(AtPositionTaskID, 1, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckInitialQuantity(ProductItem.ID, quantity);

      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, quantity);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_3_NoRFID_NoProduct_ManualTaskPass_ConsumePallet()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      int quantity = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      SetSystemCustomPropertyValue(systemIDWithoutRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      ProductItem = CreateProduct(system.LocationID, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      RunTask(AtPositionTaskID, 1, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckInitialQuantity(ProductItem.ID, quantity);

      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, quantity);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 1, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_0_NoRFID_NoProduct_ManualTaskPass_ConsumeUnitsAll()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      int quantity = 4;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      SetSystemCustomPropertyValue(systemIDWithoutRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      ProductItem = CreateProduct(system.LocationID, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      RunTask(AtPositionTaskID, 2, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 3);
      CheckMaterialConsumption(ProductItem, 1);

      SendConsumeUnit(PLC_ItemID, 2, 1); // Consume 2 units
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 1);
      CheckMaterialConsumption(ProductItem, 2);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, 1);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 4, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_3_NoRFID_NoProduct_ManualTaskPass_ConsumeUnitsAll()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      int quantity = 4;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      SetSystemCustomPropertyValue(systemIDWithoutRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      ProductItem = CreateProduct(system.LocationID, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      RunTask(AtPositionTaskID, 2, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 3);
      CheckMaterialConsumption(ProductItem, 1);

      SendConsumeUnit(PLC_ItemID, 2, 1); // Consume 2 units
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 1);
      CheckMaterialConsumption(ProductItem, 2);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, string.Empty, -1, 0);
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED, false);
        CheckItemUserState(ProductItem.ID, ItemUserState.FULLYCONSUMED);
        CheckMaterialConsumption(ProductItem, 1);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 4, PalletConsumptionType.FULL);

        CheckSetDestination(nextConv_PositionID);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_0_NoRFID_NoProduct_ManualTaskPass_ConsumeUnitsPartially()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      int quantity = 10;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      SetSystemCustomPropertyValue(systemIDWithoutRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      ProductItem = CreateProduct(system.LocationID, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      RunTask(AtPositionTaskID, 2, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 9);
      CheckMaterialConsumption(ProductItem, 1);

      SendConsumeUnit(PLC_ItemID, 4, 1); // Consume 4 units
      RunTask(AtPositionTaskID, 1, false);

      int AtPositionTaskID2 = SendAtPosition(PLC_ItemID, 1); // Remove half full pallet from conveyor
      if (RunTask(AtPositionTaskID))
      {
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
        CheckProductQuantity(ProductItem.ID, 5);
        CheckMaterialConsumption(ProductItem, 4);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 5, PalletConsumptionType.PARTIAL);

        CheckMessageUserStateByMessageID(AtPositionTaskID, TaskUserState.COMPLETE);
        CheckMessagePassFailByMessageID(AtPositionTaskID, TaskPassFail.PASS);

        if (RunTask(AtPositionTaskID2))
        {
          CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, string.Empty);
          CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, string.Empty);

          CheckMessageUserStateByMessageID(AtPositionTaskID2, TaskUserState.COMPLETE);
          CheckMessagePassFailByMessageID(AtPositionTaskID2, TaskPassFail.PASS);
        }
      }
    }

    [Test]
    public void UnitConsumptionConveyorGoodFlow_3_NoRFID_NoProduct_ManualTaskPass_ConsumeUnitsPartially()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;
      int quantity = 10;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      string foNumber = GetFOByJobID(JobID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      SetSystemCustomPropertyValue(systemIDWithoutRFID, "CP.CONSUMEPALLET", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      ProductItem = CreateProduct(system.LocationID, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID, quantity);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      RunTask(AtPositionTaskID, 2, false);

      string PLCReference = lineReference + "." + PLC_ItemID.ToString();
      CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
      CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
      CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
      CheckInitialQuantity(ProductItem.ID, quantity);

      SendConsumeUnit(PLC_ItemID, 1, 1); // Consume 1 unit
      RunTask(AtPositionTaskID, 1, false);
      CheckProductQuantity(ProductItem.ID, 9);
      CheckMaterialConsumption(ProductItem, 1);

      SendConsumeUnit(PLC_ItemID, 4, 1); // Consume 4 units
      RunTask(AtPositionTaskID, 1, false);

      int AtPositionTaskID2 = SendAtPosition(PLC_ItemID, 1); // Remove half full pallet from conveyor
      if (RunTask(AtPositionTaskID))
      {
        CheckItemUserState(ProductItem.ID, ItemUserState.PARTIALLYCONSUMED);
        CheckProductQuantity(ProductItem.ID, 5);
        CheckMaterialConsumption(ProductItem, 4);
        CheckPalletAssignedToOrder(ProductItem.UniqueID, foNumber, 5, PalletConsumptionType.PARTIAL);

        CheckMessageUserStateByMessageID(AtPositionTaskID, TaskUserState.COMPLETE);
        CheckMessagePassFailByMessageID(AtPositionTaskID, TaskPassFail.PASS);

        if (RunTask(AtPositionTaskID2))
        {
          CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, string.Empty);
          CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, string.Empty);

          CheckMessageUserStateByMessageID(AtPositionTaskID2, TaskUserState.COMPLETE);
          CheckMessagePassFailByMessageID(AtPositionTaskID2, TaskPassFail.PASS);
        }
      }
    }

    #endregion Good Flows - Conveyor without RFID reader

    #region Bad Flows - Conveyor with RFID reader

    [Test]
    public void UnitConsumptionConveyorBadFlow_InvalidPLCItemID()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 0;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_RFIDGRAI_NoProduct()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Jsa = CreateJobSystemActual(JobID, foSystemID);
      int EventDefinitionID = GetEventDefinition("ED.RFID.GRAINOSSCC", system.ID).ID;

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        CheckEventCreatedByDefinitionID(EventDefinitionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_RFIDGRAI_NoProduct()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Jsa = CreateJobSystemActual(JobID, foSystemID);
      int EventDefinitionID = GetEventDefinition("ED.RFID.GRAINOSSCC", system.ID).ID;

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        CheckEventCreatedByDefinitionID(EventDefinitionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_RFIDSSCC_NoProduct()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string SSCC = "05715333100738868";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, SSCC);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_RFIDSSCC_NoProduct()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string SSCC = "05715333100738868";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, SSCC);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_RFIDSSCC_NoPallet()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string SSCC = "05715333100738868";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Jsa = CreateJobSystemActual(JobID, foSystemID);

      ProductItem = CreateProduct(1, JobID, PLC_ItemID, -1, nextLinkedSystemID);
      ProductItem = SetProductItemSSCC(ProductItem, SSCC);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, SSCC);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_RFIDSSCC_NoPallet()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;
      string SSCC = "05715333100738868";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Jsa = CreateJobSystemActual(JobID, foSystemID);

      ProductItem = CreateProduct(1, JobID, PLC_ItemID, -1, nextLinkedSystemID);
      ProductItem = SetProductItemSSCC(ProductItem, SSCC);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, SSCC);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_RFIDTimeout_ManualTaskNoPallet()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Jsa = CreateJobSystemActual(JobID, foSystemID);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CREATETASKONREADFAILURE", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(AtPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualRFIDUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        int UserTaskID = CheckUserTask(ManualRFIDUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckEventCreatedByDefinitionID(RFIDTimeoutEventDefinitionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_RFIDTimeout_ManualTaskNoPallet()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Jsa = CreateJobSystemActual(JobID, foSystemID);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CREATETASKONREADFAILURE", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(AtPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualRFIDUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        int UserTaskID = CheckUserTask(ManualRFIDUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckEventCreatedByDefinitionID(RFIDTimeoutEventDefinitionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_RFIDTimeout_ManualTaskNoProduct()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CREATETASKONREADFAILURE", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(AtPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualRFIDUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);

        int UserTaskID = CheckUserTask(ManualRFIDUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckEventCreatedByDefinitionID(RFIDTimeoutEventDefinitionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_RFIDTimeout_ManualTaskNoProduct()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CREATETASKONREADFAILURE", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(AtPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualRFIDUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);

        int UserTaskID = CheckUserTask(ManualRFIDUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckEventCreatedByDefinitionID(RFIDTimeoutEventDefinitionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_RFIDTimeout_ManualTaskFail()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CREATETASKONREADFAILURE", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(AtPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualRFIDUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(AtPositionTaskID))
      {
        int UserTaskID = CheckUserTask(ManualRFIDUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_RFIDTimeout_ManualTaskFail()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CREATETASKONREADFAILURE", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(AtPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualRFIDUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(AtPositionTaskID))
      {
        int UserTaskID = CheckUserTask(ManualRFIDUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_RFIDTimeout_ManualTaskCancelled()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CREATETASKONREADFAILURE", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(AtPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualRFIDUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CancelUserTask(UserTask);

      if (RunTask(AtPositionTaskID))
      {
        int UserTaskID = CheckUserTask(ManualRFIDUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.CANCELLED);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_RFIDTimeout_ManualTaskCancelled()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CREATETASKONREADFAILURE", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(AtPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualRFIDUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CancelUserTask(UserTask);

      if (RunTask(AtPositionTaskID))
      {
        int UserTaskID = CheckUserTask(ManualRFIDUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.CANCELLED);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_RFIDTimeout_ManualTaskNotFound()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(AtPositionTaskID, 1, false);

      RemoveUserTaskFromAtPositionTask(AtPositionTaskID);

      if (RunTask(AtPositionTaskID))
      {
        DbTask UserTask = GetUserTaskFromAtPositionTask(AtPositionTaskID);
        Assert.IsNull(UserTask);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_RFIDTimeout_ManualTaskNotFound()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(AtPositionTaskID, 1, false);

      RemoveUserTaskFromAtPositionTask(AtPositionTaskID);

      if (RunTask(AtPositionTaskID))
      {
        DbTask UserTask = GetUserTaskFromAtPositionTask(AtPositionTaskID);
        Assert.IsNull(UserTask);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_RFIDTimeout_NoTaskCreated()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CREATETASKONREADFAILURE", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(AtPositionTaskID, 1, false);

      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_RFIDTimeout_NoTaskCreated()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CREATETASKONREADFAILURE", "0");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(AtPositionTaskID, 1, false);

      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_RFIDNoRead_NoProduct()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Jsa = CreateJobSystemActual(JobID, foSystemID);
      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, "NoRead");
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        CheckEventCreatedByDefinitionID(RFIDNoReadEventDefinitionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_RFIDNoRead_NoProduct()
    {
      Init(systemIDWithRFID, server, database, login, password);
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Jsa = CreateJobSystemActual(JobID, foSystemID);
      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, "NoRead");
      SetSystemCustomPropertyValue(systemIDWithRFID, "CP.CONSUMEPALLET", "1");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        CheckEventCreatedByDefinitionID(RFIDNoReadEventDefinitionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    #endregion Bad Flows - Conveyor with RFID reader

    #region Bad Flows - Conveyor without RFID reader

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_NoRFID_NoPallet_ManualTaskNoPallet()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_NoRFID_NoPallet_ManualTaskNoPallet()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_NoRFID_NoPallet_ManualTaskNoProduct()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_NoRFID_NoPallet_ManualTaskNoProduct()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_NoRFID_NoPallet_ManualTaskFail()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(AtPositionTaskID))
      {
        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_NoRFID_NoPallet_ManualTaskFail()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(AtPositionTaskID))
      {
        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_NoRFID_NoPallet_ManualTaskCancelled()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CancelUserTask(UserTask);

      if (RunTask(AtPositionTaskID))
      {
        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.CANCELLED);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_NoRFID_NoPallet_ManualTaskCancelled()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CancelUserTask(UserTask);

      if (RunTask(AtPositionTaskID))
      {
        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.CANCELLED);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_NoRFID_NoPallet_ManualTaskNoTaskCreated()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      RemoveUserTaskFromAtPositionTask(AtPositionTaskID);

      if (RunTask(AtPositionTaskID))
      {
        DbTask UserTask = GetUserTaskFromAtPositionTask(AtPositionTaskID);
        Assert.IsNull(UserTask);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_NoRFID_NoPallet_ManualTaskNoTaskCreated()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      RemoveUserTaskFromAtPositionTask(AtPositionTaskID);

      if (RunTask(AtPositionTaskID))
      {
        DbTask UserTask = GetUserTaskFromAtPositionTask(AtPositionTaskID);
        Assert.IsNull(UserTask);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_NoRFID_NoProduct_ManualTaskNoProduct()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_NoRFID_NoProduct_ManualTaskNoProduct()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_NoRFID_NoProduct_ManualTaskFail()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(AtPositionTaskID))
      {
        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_NoRFID_NoProduct_ManualTaskFail()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(AtPositionTaskID))
      {
        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_NoRFID_NoProduct_ManualTaskCancelled()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CancelUserTask(UserTask);

      if (RunTask(AtPositionTaskID))
      {
        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.CANCELLED);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_NoRFID_NoProduct_ManualTaskCancelled()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CancelUserTask(UserTask);

      if (RunTask(AtPositionTaskID))
      {
        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.CANCELLED);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_0_NoRFID_NoProduct_ManualTaskNoTaskCreated()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      RemoveUserTaskFromAtPositionTask(AtPositionTaskID);

      if (RunTask(AtPositionTaskID))
      {
        DbTask UserTask = GetUserTaskFromAtPositionTask(AtPositionTaskID);
        Assert.IsNull(UserTask);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void UnitConsumptionConveyorBadFlow_3_NoRFID_NoProduct_ManualTaskNoTaskCreated()
    {
      Init(systemIDWithoutRFID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      RemoveUserTaskFromAtPositionTask(AtPositionTaskID);

      if (RunTask(AtPositionTaskID))
      {
        DbTask UserTask = GetUserTaskFromAtPositionTask(AtPositionTaskID);
        Assert.IsNull(UserTask);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    #endregion Bad Flows - Conveyor without RFID reader
  }
}
