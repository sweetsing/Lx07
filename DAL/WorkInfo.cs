using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace DAL
{
   public class WorkInfo
    {
        private WorkInfo() { }
        private static WorkInfo _instance = new WorkInfo();
        public static WorkInfo Instance
        {
            get
            {
                return _instance;
            }
        }
        string cns = AppConfigurtaionServices.Configuration.GetConnectionString("cns");
        public Model.WorkInfo GetModel(string id)
        {
            using (IDbConnection cn = new MySqlConnection(cns))
            {
                string sql = "select workInfo.*,activity.activityName from workinfo join activity on workinfo.activitId=activity.activityId where workId=@id";
                return cn.QueryFirstOrDefault<Model.WorkInfoNo>(sql, new { id = id});
            }
        }
        public IEnumerable<Model.WorkInfo> GetNew()
        {
            using (IDbConnection cn = new MySqlConnection(cns))
            {
                string sql = "select * from workinfo where workVerify='审核通过' order by uploadTime desc limit 8";
                return cn.Query<Model.WorkInfo>(sql);
            }
        }
        public IEnumerable<Model.WorkInfo> GetRecmmend()
        {
            using (IDbConnection cn = new MySqlConnection(cns))
            {
                string sql = "select * from workinfo where workVerify='审核通过' and recommend='是' order by recommendTime desc limit 6";
                return cn.Query<Model.WorkInfo>(sql);
            }
        }

        public int GetCount()
        {
            using (IDbConnection cn = new MySqlConnection(cns))
            {
                string sql = "select count(1) from workinfo";
                return cn.ExecuteScalar<int>(sql);
            }
        }
        public int GetCount(int[] activityIds)
        {
            using (IDbConnection cn = new MySqlConnection(cns))
            {
                string sql = "select count(1) from workinfo in @activityIds";
                return cn.ExecuteScalar<int>(sql,new { ActivityIds=activityIds});
            }
        }

        public IEnumerable<Model.WorkInfo> GetPage(Model.WorkPage page)
        {
            using (IDbConnection cn = new MySqlConnection(cns))
            {
                string sql = "with a as(select row_number() over(order by uploadTime) as num, workinfo.* from workinfo where activityIds in @activityIds)";
                sql += "select* from a where num between (@pageIndex-1)*@pageSize+1 and @pageIndex*@pageSize;";
                return cn.Query<Model.WorkInfo>(sql,new { pageIndex=page.pageIndex,pageSize=page.pageSize,activityIds=page.activityIds});
            }
        }
        public int GetFindCount(string workName)
        {
            using (IDbConnection cn = new MySqlConnection(cns))
            {
                string sql = "select count(1) from workinfo where workVerify='审核通过' and workName like concat('%',@workName,'%')";
                return cn.ExecuteScalar<int>(sql,new { workName =workName});
            }
        }
        public IEnumerable<Model.WorkInfo> GetFindPage(Model.WorkFindPage page)
        {
            using (IDbConnection cn = new MySqlConnection(cns))
            {
                string sql = "with a as(select row_number() over(order by uploadTime desc) as num, workinfo.*,activityName from workinfo join activity on  workinfo.activityId=activity.activityId where workverify='审核通过' and workName like concat('%',@workName,'%'))";
                sql += "select* from a where num between (@pageIndex-1)*@pageSize+1 and @pageIndex*@pageSize";
                return cn.Query<Model.WorkInfoNo>(sql,page);
            }
        }

        public int GetMyCount(string userName)
        {
            using (IDbConnection cn = new MySqlConnection(cns))
            {
                string sql = "select count(1) from workinfo where userName=@userName";
                return cn.ExecuteScalar<int>(sql,new { userName = userName });
            }
        }
        public IEnumerable<Model.WorkInfo> GetNyPage(Model.WorkFindPage page)
        {
            using (IDbConnection cn = new MySqlConnection(cns))
            {
                string sql = "with a as(select row_number() over(order by uploadTime desc) as num, workinfo.*,activityName from workinfo join activity on  workinfo.activityId=activity.activityId where workinfo.userName=@userName)";
                sql += "select* from a where num between (@pageIndex-1)*@pageSize+1 and @pageIndex*@pageSize";
                return cn.Query<Model.WorkInfoNo>(sql, page);
            }
        }

        public int Add(Model.WorkInfo workinfo)
        {
            using (IDbConnection cn = new MySqlConnection(cns))
            {
                string sql = "insert into workinfo values(workId,workName,workPicture,uploadTime,workIntroduction,workVerify,userName,activityId,recommend,recommendTime)"+ "value(@workId,@workName,@workPicture,@uploadTime,@workIntroduction,@workVerify,@userName,@activityId,@recommend,@recommendTime）";
                sql += "SELECT @@IDENTITY";
                return cn.ExecuteScalar<int>(sql, workinfo);
            }
        }
        public int Update(Model.WorkInfo workinfo)
        {
            using (IDbConnection cn = new MySqlConnection(cns))
            {
                string sql = "update workinfo set workName=@workName,workPicture=@workPicture,uploadTime=@uploadTime,workIntroduction=@workIntroduction,workVerify=@workVerify"+ "userName=@userName,recommend=@recommend,recommendTime=@recommendTime where workId=@workId";
                return cn.Execute(sql, workinfo);
            }
        }
        public int UpdateImg(Model.WorkInfo workinfo)
        {
            using (IDbConnection cn = new MySqlConnection(cns))
            {
                string sql = "update workinfo set workPicture=@workPicture where workId=@workId";
                return cn.Execute(sql, workinfo);
            }
        }
        public int UpdateVerify(Model.WorkInfo workinfo)
        {
            using (IDbConnection cn = new MySqlConnection(cns))
            {
                string sql = "update workinfo set workPicture=@workPicture where workId=@workId";
                if (workinfo.workPicture == null)
                    sql += "update workinfo set workVerify=@workVerify where workId=@workId";
                return cn.Execute(sql, workinfo);
            }
        }
        public int Delete(int id)
        {
            using (IDbConnection cn = new MySqlConnection(cns))
            {
                string sql = "delete from workinfo where workid=@id";
                return cn.Execute(sql, new { id = id });
            }
        }
        public int UpdateRecommend(Model.WorkInfo workinfo)
        {
            using (IDbConnection cn = new MySqlConnection(cns))
            {
                string sql = "update workinfo set recommend=@recommend,recommendTime=@recommendTime where workId=@workId";
                return cn.Execute(sql, workinfo);
            }
        }
    }
}
}
