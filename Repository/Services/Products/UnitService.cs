using ERP.Repository.Configuration.Enum;
using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.Configuration.Validation;
using ERP.Repository.Interface.AuditLogs;
using ERP.Repository.Interface.Data.ProductData;
using ERP.Repository.Interface.Products;
using ERP.Repository.Model.Products;
using ERP.Repository.ViewModel.Products;
using FluentValidation;

namespace ERP.Repository.Services.Products
{
    // Units of measurement. The defaults are seeded by the migration; the owner can add, rename and delete
    // units, but not delete one that products still use.
    public class UnitService(
        IUnitData _unit,
        IAuditLogService _auditLog,
        IValidator<UnitPostViewModel> _postValidator,
        IValidator<UnitUpdateViewModel> _updateValidator
    ) : IUnitService
    {
        public async Task<UnitPageViewModel> GetUnits(
            int page,
            int pageSize,
            string? name,
            int filter,
            CancellationToken cancellation
        )
        {
            PageQueryValidator.Ensure(page, pageSize);

            var units = await _unit.GetAllUnitsWithoutTracking(page, pageSize, name, filter, cancellation);
            var count = await _unit.UnitTotalCount(name, cancellation);

            return new UnitPageViewModel
            {
                Units = units,
                PageCount = (int)Math.Ceiling(count / (double)pageSize),
                Rows = count,
            };
        }

        public async Task InsertUnit(UnitPostViewModel unit)
        {
            await _postValidator.EnsureValidAsync(unit);

            if (await _unit.CheckExistingUnit(unit.Name!))
            {
                throw new ValidationFailed("name", "A unit with this name already exists.");
            }

            var now = DateTime.UtcNow;

            var u = new Unit
            {
                Name = unit.Name!.Trim().ToLower(),
                Created_By = _auditLog.CurrentUserId,
                Created_At = now,
                IsActive = true,
            };

            await _unit.Save(u);

            _auditLog.Log(AuditModuleEnum.Unit, AuditActionEnum.Create, $"Added unit '{u.Name}'", u.Id, now);
            await _unit.SaveChanges();
        }

        public async Task UpdateUnit(UnitUpdateViewModel unit)
        {
            await _updateValidator.EnsureValidAsync(unit);

            var u = await _unit.GetUnitByIdWithTracking(unit.Id);

            if (u == null)
            {
                throw new NotFound("Unit not found");
            }

            if (await _unit.CheckExistingUnit(unit.Name!, unit.Id))
            {
                throw new ValidationFailed("name", "A unit with this name already exists.");
            }

            var now = DateTime.UtcNow;
            var oldName = u.Name;

            u.Name = unit.Name!.Trim().ToLower();
            u.Updated_By = _auditLog.CurrentUserId;
            u.Updated_At = now;

            _auditLog.Log(
                AuditModuleEnum.Unit,
                AuditActionEnum.Update,
                $"Renamed unit '{oldName}' to '{u.Name}'",
                u.Id,
                now
            );

            await _unit.SaveChanges();
        }

        public async Task DeleteUnit(int id)
        {
            if (id <= 0)
            {
                throw new BadRequest("Unit ID is required");
            }

            var u = await _unit.GetUnitByIdWithTracking(id);

            if (u == null)
            {
                throw new NotFound($"Unit with ID {id} not found.");
            }

            var used = await _unit.ProductCount(id);

            if (used > 0)
            {
                throw new BadRequest(
                    $"'{u.Name}' is used by {used} product(s). Change their unit first, then delete it."
                );
            }

            var now = DateTime.UtcNow;

            u.IsActive = false;
            u.Deleted_By = _auditLog.CurrentUserId;
            u.Deleted_At = now;

            _auditLog.Log(AuditModuleEnum.Unit, AuditActionEnum.Delete, $"Deleted unit '{u.Name}'", u.Id, now);

            await _unit.SaveChanges();
        }
    }
}
