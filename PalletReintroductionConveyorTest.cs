using ETS.Core.Api.Models.Data;
using ETS.Core.Enums;
using NUnit.Framework;
using System.Threading;

namespace ConveyorProcessLogicTest
{
  public class PalletReintroductionConveyorTest : TestMethod
  {
    const string server = "192.168.22.101";
    const string database = "RW_WCS_CORE";
    const string login = "edbApp";
    const string password = "Sqlapp!23";
    const int systemID = 177;
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
    public void PalletReintroductionConveyorGoodFlow_0_NormalPallet()
    {
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemReference);

        CheckSetDestination(nextConv_PositionID);

        CheckRFIDTags(string.Empty);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletReintroductionConveyorGoodFlow_3_NormalPallet()
    {
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemReference);

        CheckSetDestination(nextConv_PositionID);

        CheckRFIDTags(string.Empty);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletReintroductionConveyorGoodFlow_0_LostPallet()
    {
      PLC_ItemID = 2345;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemReference);

        CheckSetDestination(nextConv_PositionID);

        CheckRFIDTags(string.Empty);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletReintroductionConveyorGoodFlow_3_LostPallet()
    {
      PLC_ItemID = 2345;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemReference);

        CheckSetDestination(nextConv_PositionID);

        CheckRFIDTags(string.Empty);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletReintroductionConveyorGoodFlow_0_ReintroPalletGRAI_ProductExists()
    {
      PLC_ItemID = 1234;
      JobID = 4161;
      string GRAI = "1234567890";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, alternateDestinationSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemReference, alternateDestinationSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckRFIDTags(string.Empty);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletReintroductionConveyorGoodFlow_3_ReintroPalletGRAI_ProductExists()
    {
      PLC_ItemID = 1234;
      JobID = 4161;
      string GRAI = "1234567890";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, alternateDestinationSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemReference, alternateDestinationSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckRFIDTags(string.Empty);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletReintroductionConveyorGoodFlow_0_ReintroPalletGRAI_NoProduct_UserTaskPass()
    {
      PLC_ItemID = 1234;
      JobID = 4161;
      string GRAI = "1234567890";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 3, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      ProductItem = CreateProduct(PalletItem.LocationID, JobID, PLC_ItemID, PalletItem.ID);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, nextLinkedSystemID);
        ProductItem = GetProductItem(PalletItem);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemReference, nextLinkedSystemID);
        
        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);
 
        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletReintroductionConveyorGoodFlow_3_ReintroPalletGRAI_NoProduct_UserTaskPass()
    {
      PLC_ItemID = 1234;
      JobID = 4161;
      string GRAI = "1234567890";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 3, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      ProductItem = CreateProduct(PalletItem.LocationID, JobID, PLC_ItemID, PalletItem.ID);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, nextLinkedSystemID);
        ProductItem = GetProductItem(PalletItem);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemReference, nextLinkedSystemID);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletReintroductionConveyorGoodFlow_0_ReintroPalletNoGRAI_UserTaskPass()
    {
      PLC_ItemID = 1234;
      JobID = 4161;
      string GRAI = "1234567890";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      CreateJobSystemActual(JobID, foSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 3, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      PalletItem = GetPalletItemByPLCItemID(PLC_ItemID);
      ProductItem = CreateProduct(PalletItem.LocationID, JobID, PLC_ItemID, PalletItem.ID);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, nextLinkedSystemID);
        ProductItem = GetProductItem(PalletItem);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemReference, nextLinkedSystemID);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletReintroductionConveyorGoodFlow_3_ReintroPalletNoGRAI_UserTaskPass()
    {
      PLC_ItemID = 1234;
      JobID = 4161;
      string GRAI = "1234567890";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      CreateJobSystemActual(JobID, foSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 3, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      PalletItem = GetPalletItemByPLCItemID(PLC_ItemID);
      ProductItem = CreateProduct(PalletItem.LocationID, JobID, PLC_ItemID, PalletItem.ID);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, nextLinkedSystemID);
        ProductItem = GetProductItem(PalletItem);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemReference, nextLinkedSystemID);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletReintroductionConveyorGoodFlow_0_ReintroPalletRFIDTimeout_UserTaskPass()
    {
      PLC_ItemID = 1234;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      CreateJobSystemActual(JobID, foSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      Thread.Sleep(rfidReaderTimeout * 1000); // wait for timeout

      RunTask(AtPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      ProductItem = CreateProduct(PalletItem.LocationID, JobID, PLC_ItemID, PalletItem.ID);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, nextLinkedSystemID);
        ProductItem = GetProductItem(PalletItem);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemReference, nextLinkedSystemID);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletReintroductionConveyorGoodFlow_3_ReintroPalletRFIDTimeout_UserTaskPass()
    {
      PLC_ItemID = 1234;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      CreateJobSystemActual(JobID, foSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      Thread.Sleep(rfidReaderTimeout * 1000); // wait for timeout

      RunTask(AtPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      ProductItem = CreateProduct(PalletItem.LocationID, JobID, PLC_ItemID, PalletItem.ID);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, nextLinkedSystemID);
        ProductItem = GetProductItem(PalletItem);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemReference, nextLinkedSystemID);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletReintroductionConveyorGoodFlow_1()
    {
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 1);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCItemReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, PLCItemReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletReintroductionConveyorGoodFlow_2()
    {
      PLC_ItemID = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 2);

      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletReintroductionConveyorGoodFlow_4()
    {
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 4);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, -1, PLCItemReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, -1, PLCItemReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    #endregion Good Flows

    #region Bad Flows

    [Test]
    public void PalletReintroductionConveyorBadFlow_0_InvalidPLCItemID()
    {
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
    public void PalletReintroductionConveyorBadFlow_0_NormalPallet_NoPallet()
    {
      PLC_ItemID = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckRFIDTags(string.Empty);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_3_NormalPallet_NoPallet()
    {
      PLC_ItemID = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckRFIDTags(string.Empty);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_0_NormalPallet_NoProduct()
    {
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, ejectDestinationSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckRFIDTags(string.Empty);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_3_NormalPallet_NoProduct()
    {
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, ejectDestinationSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckRFIDTags(string.Empty);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_0_LostPallet_NoPallet()
    {
      PLC_ItemID = 2345;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckRFIDTags(string.Empty);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_3_LostPallet_NoPallet()
    {
      PLC_ItemID = 2345;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckRFIDTags(string.Empty);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_0_LostPallet_NoProduct()
    {
      PLC_ItemID = 2345;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, ejectDestinationSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckRFIDTags(string.Empty);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_3_LostPallet_NoProduct()
    {
      PLC_ItemID = 2345;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, ejectDestinationSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckRFIDTags(string.Empty);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_0_ReintroPallet_NoPallet()
    {
      PLC_ItemID = 1234;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckRFIDTags(string.Empty);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_3_ReintroPallet_NoPallet()
    {
      PLC_ItemID = 1234;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckRFIDTags(string.Empty);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_0_ReintroPalletGRAI_NoProduct_UserTaskFail()
    {
      PLC_ItemID = 1234;
      JobID = 4161;
      string GRAI = "1234567890";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 3, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      ProductItem = CreateProduct(PalletItem.LocationID, JobID, PLC_ItemID, PalletItem.ID);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, ejectDestinationSystemID);
        ProductItem = GetProductItem(PalletItem);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemReference, ejectDestinationSystemID);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_3_ReintroPalletGRAI_NoProduct_UserTaskFail()
    {
      PLC_ItemID = 1234;
      JobID = 4161;
      string GRAI = "1234567890";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI); ;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 3, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      ProductItem = CreateProduct(PalletItem.LocationID, JobID, PLC_ItemID, PalletItem.ID);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, ejectDestinationSystemID);
        ProductItem = GetProductItem(PalletItem);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemReference, ejectDestinationSystemID);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_0_ReintroPalletGRAI_NoProduct_NoUserTask()
    {
      PLC_ItemID = 1234;
      JobID = 4161;
      string GRAI = "1234567890";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 3, false);

      // Remove user task
      RemoveUserTaskFromAtPositionTask(AtPositionTaskID);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, ejectDestinationSystemID);

        DbTask UserTask = GetUserTaskFromAtPositionTask(AtPositionTaskID);
        Assert.IsNull(UserTask);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_3_ReintroPalletGRAI_NoProduct_NoUserTask()
    {
      PLC_ItemID = 1234;
      JobID = 4161;
      string GRAI = "1234567890";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 3, false);

      // Remove user task
      RemoveUserTaskFromAtPositionTask(AtPositionTaskID);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, ejectDestinationSystemID);

        DbTask UserTask = GetUserTaskFromAtPositionTask(AtPositionTaskID);
        Assert.IsNull(UserTask);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_0_ReintroPalletNoGRAI_UserTaskFail()
    {
      PLC_ItemID = 1234;
      JobID = 4161;
      string GRAI = "1234567890";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      CreateJobSystemActual(JobID, foSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 3, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      PalletItem = GetPalletItemByPLCItemID(PLC_ItemID);
      ProductItem = CreateProduct(PalletItem.LocationID, JobID, PLC_ItemID, PalletItem.ID);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, ejectDestinationSystemID);
        ProductItem = GetProductItem(PalletItem);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemReference, ejectDestinationSystemID);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_3_ReintroPalletNoGRAI_UserTaskFail()
    {
      PLC_ItemID = 1234;
      JobID = 4161;
      string GRAI = "1234567890";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      CreateJobSystemActual(JobID, foSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 3, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      PalletItem = GetPalletItemByPLCItemID(PLC_ItemID);
      ProductItem = CreateProduct(PalletItem.LocationID, JobID, PLC_ItemID, PalletItem.ID);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, ejectDestinationSystemID);
        ProductItem = GetProductItem(PalletItem);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemReference, ejectDestinationSystemID);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_0_ReintroPalletNoGRAI_NoUserTask()
    {
      PLC_ItemID = 1234;
      JobID = 4161;
      string GRAI = "1234567890";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      CreateJobSystemActual(JobID, foSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 3, false);

      // Remove user task
      RemoveUserTaskFromAtPositionTask(AtPositionTaskID);
      PalletItem = GetPalletItemByPLCItemID(PLC_ItemID);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, ejectDestinationSystemID);

        DbTask UserTask = GetUserTaskFromAtPositionTask(AtPositionTaskID);
        Assert.IsNull(UserTask);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_3_ReintroPalletNoGRAI_NoUserTask()
    {
      PLC_ItemID = 1234;
      JobID = 4161;
      string GRAI = "1234567890";

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);
      CreateJobSystemActual(JobID, foSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 3, false);

      // Remove user task
      RemoveUserTaskFromAtPositionTask(AtPositionTaskID);
      PalletItem = GetPalletItemByPLCItemID(PLC_ItemID);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, ejectDestinationSystemID);

        DbTask UserTask = GetUserTaskFromAtPositionTask(AtPositionTaskID);
        Assert.IsNull(UserTask);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_0_ReintroPalletRFIDTimeout_UserTaskFail()
    {
      PLC_ItemID = 1234;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      CreateJobSystemActual(JobID, foSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      Thread.Sleep(rfidReaderTimeout * 1000); // wait for timeout

      RunTask(AtPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      ProductItem = CreateProduct(PalletItem.LocationID, JobID, PLC_ItemID, PalletItem.ID);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, ejectDestinationSystemID);
        ProductItem = GetProductItem(PalletItem);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemReference, ejectDestinationSystemID);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_3_ReintroPalletRFIDTimeout_UserTaskFail()
    {
      PLC_ItemID = 1234;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      CreateJobSystemActual(JobID, foSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      Thread.Sleep(rfidReaderTimeout * 1000); // wait for timeout

      RunTask(AtPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      ProductItem = CreateProduct(PalletItem.LocationID, JobID, PLC_ItemID, PalletItem.ID);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, ejectDestinationSystemID);
        ProductItem = GetProductItem(PalletItem);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemReference, ejectDestinationSystemID);

        int UserTaskID = CheckUserTask(ManualUserTaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_0_ReintroPalletRFIDTimeout_NoUserTask()
    {
      PLC_ItemID = 1234;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      CreateJobSystemActual(JobID, foSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      Thread.Sleep(rfidReaderTimeout * 1000); // wait for timeout

      RunTask(AtPositionTaskID, 1, false);

      // Remove user task
      RemoveUserTaskFromAtPositionTask(AtPositionTaskID);
      PalletItem = GetPalletItemByPLCItemID(PLC_ItemID);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, ejectDestinationSystemID);

        DbTask UserTask = GetUserTaskFromAtPositionTask(AtPositionTaskID);
        Assert.IsNull(UserTask);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletReintroductionConveyorBadFlow_3_ReintroPalletRFIDTimeout_NoUserTask()
    {
      PLC_ItemID = 1234;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      CreateJobSystemActual(JobID, foSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      Thread.Sleep(rfidReaderTimeout * 1000); // wait for timeout

      RunTask(AtPositionTaskID, 1, false);

      // Remove user task
      RemoveUserTaskFromAtPositionTask(AtPositionTaskID);
      PalletItem = GetPalletItemByPLCItemID(PLC_ItemID);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemReference, ejectDestinationSystemID);

        DbTask UserTask = GetUserTaskFromAtPositionTask(AtPositionTaskID);
        Assert.IsNull(UserTask);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    #endregion Bad Flows
  }
}
