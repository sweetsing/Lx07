using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Service.Controllers
{
    [Route("api/[controller]")]
    public class WorkController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public WorkController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public ActionResult Get()
        {
            return Json(DAL.WorkInfo.Instance.GetCount());
        }
        [HttpGet("new")]
        public ActionResult GetNew()  
        {
            var result = DAL.Activity.Instance.GetNew();
            if (result.Count() != 0)
                return Json(Result.Ok(result));
            else
                return Json(Result.Err("记录数为0"));
        }
        [HttpGet("{id}")]
        public ActionResult Get(int id) 
        {
            var result = DAL.Activity.Instance.GetModel(id);
            if (result != null)
                return Json(Result.Ok(result));
            else
                return Json(Result.Err("WorkId不存在"));
        }

        [HttpPost]
        public ActionResult Post([FromBody]Model.WorkInfo workInfo)  
        {
            workInfo.recommend = "否";
            workInfo.workVerify = "待审核";
            workInfo.uploadTime = DateTime.Now;

            try
            {
                int n = DAL.WorkInfo.Instance.Add(workInfo);
                return Json(Result.Ok("发布活动成功", n));
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("foreign key"))
                    if (ex.Message.ToLower().Contains("username"))
                        return Json(Result.Err("合法用户才能添加记录"));
                    else
                        return Json(Result.Err("作品所属活动不存在"));
                else if (ex.Message.ToLower().Contains("null"))
                    return Json(Result.Err("活动名称、结束时间、活动图片、活动审核情况、用户名不能为空"));
                else
                    return Json(Result.Err(ex.Message));
            }
        }

        [HttpPut]
        public ActionResult Put([FromBody]Model.WorkInfo workInfo) 
        {
            workInfo.recommend = "否";
            workInfo.workVerify = "待审核";
            workInfo.uploadTime = DateTime.Now;
            try
            {
                var n = DAL.WorkInfo.Instance.Update(workInfo);
                if (n != 0)
                    return Json(Result.Ok("修改活动成功", workInfo.activityId));
                else
                    return Json(Result.Err("workId不存在"));
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("null"))
                    return Json(Result.Err("活动名称、结束时间、活动图片、活动审核情况不能为空"));
                else
                    return Json(Result.Err(ex.Message));
            }
        }
        [HttpGet("findCount")]
        public ActionResult getFindCount(string findName)
        {
            if (findName == null) findName = "";
            return Json(DAL.WorkInfo.Instance.GetFindCount(findName));
        }
        [HttpGet("myCount")]
        public ActionResult getMyCount(string userName)
        {
            if (userName == null) userName = "";
            return Json(DAL.WorkInfo.Instance.GetFindCount(userName));
        }
        [HttpPost("findPage")]
        public ActionResult getFindPage([FromBody] Model.WorkFindPage page)
        {
            if (page.workName == null) page.workName = "";
            var result = DAL.WorkInfo.Instance.GetFindPage(page);
            if (result.Count() == 0)
                return Json(Result.Err("返回记录"));
            else
                return Json(Result.Ok(result));
        }
        [HttpPost("myPage")]
        public ActionResult getMyPage([FromBody] Model.WorkFindPage page)
        {
            if (page.workName == null) page.workName = "";
            var result = DAL.WorkInfo.Instance.GetFindPage(page);
            if (result.Count() == 0)
                return Json(Result.Err("返回记录"));
            else
                return Json(Result.Ok(result));
        }
        [HttpPut("Verify")]
        public ActionResult PutVerify([FromBody] Model.WorkInfo workInfo)
        {
            try
            {
                var n = DAL.WorkInfo.Instance.Update(workInfo);
                if (n != 0)
                    return Json(Result.Ok("审核作品成功", workInfo.workId));
                else
                    return Json(Result.Err("workId不存在"));
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("null"))
                    return Json(Result.Err("作品审核情况不能为空"));
                else
                    return Json(Result.Err(ex.Message));
            }
        }
        [HttpPut("Recommend")]
        public ActionResult PutRecommend([FromBody] Model.WorkInfo workInfo)
        {
            workInfo.recommendTime = DateTime.Now;
            try
            {
                var re = "";
                if (workInfo.recommend == "否") re = "取消";
                var n = DAL.WorkInfo.Instance.UpdateRecommend(workInfo);
                if (n != 0)
                    return Json(Result.Ok($"{re}推送作品成功", workInfo.workId));
                else
                    return Json(Result.Err("workId不存在"));
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("null"))
                    return Json(Result.Err("推荐作品情况不能为空"));
                else
                    return Json(Result.Err(ex.Message));
            }
        }
        [HttpPut("id")]
        public ActionResult upImg(string id, List<IFormFile> files)
        {
            var path = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "img", "Work");
            var fileName = $"{path}/{id}";
            try
            {
                var ext = DAL.Upload.Instance.UpImg(files[0], fileName);
                if (ext == null)
                    return Json(Result.Err("请上传图片文件"));
                else
                {
                    var file = $"img/Work/{id}{ext}";
                    Model.WorkInfo workInfo = new Model.WorkInfo();
                    if (id.StartsWith("i"))
                    {
                        workInfo.workId = int.Parse(id.Substring(1));
                        workInfo.workIntroduction = file;
                    }
                    else
                    {
                        workInfo.workId = int.Parse(id);
                        workInfo.workIntroduction = file;
                    }
                    var n = DAL.WorkInfo.Instance.UpdateImg(workInfo);
                    if (n > 0)
                        return Json(Result.Ok("上传成功"));
                    else
                        return Json(Result.Err("请输入正确的作品id"))l
                }
            }
            catch(Exception ex)
            {
                return Json(Result.Err(ex.Message));
            }
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)  //删除活动
        {
            try
            {
                var n = DAL.Activity.Instance.Delete(id);
                if (n != 0)
                    return Json(Result.Ok("删除成功"));
                else
                    return Json(Result.Err("activityID不存在"));

            }
            catch (Exception ex)
            {
                return Json(Result.Err(ex.Message));
            }
        }

        [HttpPost("page")]  //分页获取活动数据
        public ActionResult getPage([FromBody] Model.Page page)
        {
            var result = DAL.Activity.Instance.GetPage(page);
            if (result.Count() == 0)
                return Json(Result.Err("返回记录数为0"));
            else
                return Json(Result.Ok(result));
        }
        [HttpPost("verifyPage")]  //分页获取审核通过活动数据
        public ActionResult getVerifyPage([FromBody] Model.Page page)
        {
            var result = DAL.Activity.Instance.GetVerifyPage(page);
            if (result.Count() == 0)
                return Json(Result.Err("返回记录数为0"));
            else
                return Json(Result.Ok(result));
        }
        [HttpPut("Verify")]
        public ActionResult PutVerify([FromBody]Model.Activity activity)  //修改活动审核情况
        {
            try
            {
                var n = DAL.Activity.Instance.UpdateVerify(activity);
                if (n != 0)
                    return Json(Result.Ok("审核活动成功", activity.activityId));
                else
                    return Json(Result.Err("activityId不存在"));
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("null"))
                    return Json(Result.Err("活动审核情况不能为空"));
                else
                    return Json(Result.Err(ex.Message));
            }
        }

        [HttpPut("Recommend")]
        public ActionResult PutRecommend([FromBody]Model.Activity activity)   //修改活动推荐情况
        {
            activity.recommendTime = DateTime.Now;
            try
            {
                var re = "";
                if (activity.recommend == "否") re = "取消";
                var n = DAL.Activity.Instance.UpdateRecommend(activity);

                if (n != 0)
                    return Json(Result.Ok($"{re}推荐活动成功", activity.activityId));
                else
                    return Json(Result.Err("activityId不存在"));
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("null"))
                    return Json(Result.Err("推荐活动情况不能为空"));
                else
                    return Json(Result.Err(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public ActionResult upImg(int id, List<IFormFile> files)
        {

            var path = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "img", "Activity");
            var fileName = $"{path}/{id}";
            try
            {
                var ext = DAL.Upload.Instance.UpImg(files[0], fileName);
                if (ext == null)
                    return Json(Result.Err("请上传图片文件"));
                else
                {
                    var file = $"img/Activity/{id}{ext}";
                    Model.Activity activity = new Model.Activity() { activityId = id, activityPicture = file };
                    var n = DAL.Activity.Instance.UpdateImg(activity);
                    if (n > 0)
                        return Json(Result.Ok("上传成功", file));
                    else
                        return Json(Result.Err("请输入正确的活动id"));
                }
            }
            catch (Exception ex)
            {
                return Json(Result.Err(ex.Message));
            }
        }







        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
