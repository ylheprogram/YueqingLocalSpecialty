﻿using BLL.Repository;
using Global;
using System.Web;

namespace SRV.ProductionService
{
    public class TransactionControl
    {
        private static SqlDbContext GetCurrentRequestDbContext()
        {
            SqlDbContext dbContext = HttpContext.Current.Items[Key.DbContext] as SqlDbContext;
            // 防止子Action关闭事务时出现异常
            HttpContext.Current.Items.Remove(Key.DbContext);
            return dbContext;
        }

        public static void Commit()
        {
            SqlDbContext dbContext = GetCurrentRequestDbContext();
            if (dbContext != null)
            {
                using (dbContext)
                {
                    // 在事务结束前统一保存更改
                    dbContext.SaveChanges();
                    dbContext.Database.CurrentTransaction.Commit();
                }
            }
        }

        public static void Rollback()
        {
            SqlDbContext dbContext = GetCurrentRequestDbContext();
            if (dbContext != null)
            {
                using (dbContext)
                {
                    // 在事务结束前统一保存更改
                    dbContext.SaveChanges();
                    dbContext.Database.CurrentTransaction.Rollback();
                }
            }
        }
    }
}
