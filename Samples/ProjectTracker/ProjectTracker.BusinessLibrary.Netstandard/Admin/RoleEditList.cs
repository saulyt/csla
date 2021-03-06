using Csla;
using Csla.Data;
using Csla.Security;
using System;
using Csla.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectTracker.Library
{
  namespace Admin
  {
    /// <summary>
    /// Used to maintain the list of roles
    /// in the system.
    /// </summary>
    [Serializable()]
    public class RoleEditList : BusinessListBase<RoleEditList, RoleEdit>
    {
      /// <summary>
      /// Remove a role based on the role's
      /// id value.
      /// </summary>
      /// <param name="id">Id value of the role to remove.</param>
      public void Remove(int id)
      {
        foreach (RoleEdit item in this)
        {
          if (item.Id == id)
          {
            Remove(item);
            break;
          }
        }
      }

      /// <summary>
      /// Get a role bsaed on its id value.
      /// </summary>
      /// <param name="id">Id value of the role to return.</param>
      public RoleEdit GetRoleById(int id)
      {
        foreach (RoleEdit item in this)
        {
          if (item.Id == id)
          {
            return item;
          }
        }
        return null;
      }

      [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
      public static void AddObjectAuthorizationRules()
      {
        Csla.Rules.BusinessRules.AddRule(typeof(RoleEditList), new Csla.Rules.CommonRules.IsInRole(Csla.Rules.AuthorizationActions.CreateObject, Security.Roles.Administrator));
        Csla.Rules.BusinessRules.AddRule(typeof(RoleEditList), new Csla.Rules.CommonRules.IsInRole(Csla.Rules.AuthorizationActions.EditObject, Security.Roles.Administrator));
        Csla.Rules.BusinessRules.AddRule(typeof(RoleEditList), new Csla.Rules.CommonRules.IsInRole(Csla.Rules.AuthorizationActions.DeleteObject, Security.Roles.Administrator));
      }

      public static async Task<RoleEditList> GetRolesAsync()
      {
        return await DataPortal.FetchAsync<RoleEditList>();
      }

      public RoleEditList()
      {
        this.Saved += Roles_Saved;
        this.AllowNew = true;
      }

      private void Roles_Saved(object sender, Csla.Core.SavedEventArgs e)
      {
        // this runs on the client and invalidates
        // the RoleList cache
        RoleList.InvalidateCache();
      }

      protected override RoleEdit AddNewCore()
      {
        RoleEdit item = DataPortal.CreateChild<RoleEdit>();
        Add(item);
        return item;
      }

      public static RoleEditList GetRoles()
      {
        return DataPortal.Fetch<RoleEditList>();
      }

      protected override void OnDeserialized()
      {
        base.OnDeserialized();
        this.Saved += Roles_Saved;
      }

      protected override void DataPortal_OnDataPortalInvokeComplete(Csla.DataPortalEventArgs e)
      {
        if (ApplicationContext.ExecutionLocation == ApplicationContext.ExecutionLocations.Server &&
            e.Operation == DataPortalOperations.Update)
        {
          // this runs on the server and invalidates
          // the RoleList cache
          RoleList.InvalidateCache();
        }
      }

      [Fetch]
      private void DataPortal_Fetch()
      {
        var rlce = RaiseListChangedEvents;
        RaiseListChangedEvents = false;
        using (var ctx = ProjectTracker.Dal.DalFactory.GetManager())
        {
          var dal = ctx.GetProvider<ProjectTracker.Dal.IRoleDal>();
          List<ProjectTracker.Dal.RoleDto> list = null;
          list = dal.Fetch();
          foreach (var item in list)
            Add(DataPortal.FetchChild<RoleEdit>(item));
        }
        RaiseListChangedEvents = rlce;
      }

      [Update]
      [Transactional(TransactionalTypes.TransactionScope)]
      protected override void DataPortal_Update()
      {
        this.RaiseListChangedEvents = false;
        using (var ctx = ProjectTracker.Dal.DalFactory.GetManager())
          Child_Update();
        this.RaiseListChangedEvents = true;
      }
    }
  }
}