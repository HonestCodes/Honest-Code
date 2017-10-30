using GSwap.Data;
using GSwap.Data.Providers;
using GSwap.Models.Domain.CMS;
using GSwap.Models.Requests.CMS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSwap.Services
{
    public class CMSService : ICMSService
    {

        private IDataProvider _dataProvider;

        public CMSService(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;

        }
        //--------------------------------cms templates--------------------------------------------------//
        public int PostTemplate(CMSTemplateAddRequest model, int userId)
        {
            int id = 0;

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Name", model.Name);
                paramCollection.AddWithValue("@Description", model.Description);
                paramCollection.AddWithValue("@Path", model.Path);
                paramCollection.AddWithValue("@CreatedBy", userId);
                paramCollection.AddWithValue("@ModifiedBy", userId);

                SqlParameter idParameter = new SqlParameter("@Id", System.Data.SqlDbType.Int);
                idParameter.Direction = System.Data.ParameterDirection.Output;

                paramCollection.Add(idParameter);
            };

            Action<SqlParameterCollection> returnParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                Int32.TryParse(paramCollection["@Id"].Value.ToString(), out id);

            };

            string proc = "dbo.CMSTemplates_Insert";
            _dataProvider.ExecuteNonQuery(proc, inputParamDelegate, returnParamDelegate);

            return id;
        }

        public List<CMSTemplate> GetTemplates()
        {
            List<CMSTemplate> list = null;

            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {
                CMSTemplate myTemplate = MapTemplate(reader);

                if (list == null)
                {
                    list = new List<CMSTemplate>();
                }

                list.Add(myTemplate);
            };
            Action<SqlParameterCollection> inputParamDelegate = null;

            _dataProvider.ExecuteCmd("dbo.CMSTemplates_SelectAll", inputParamDelegate, singleRecMapper);
            return list;

        }

        private static CMSTemplate MapTemplate(IDataReader reader)
        {
            CMSTemplate myTemplate = new CMSTemplate();
            int startingIndex = 0;

            myTemplate.Id = reader.GetSafeInt32(startingIndex++);
            myTemplate.Name = reader.GetSafeString(startingIndex++);
            myTemplate.Description = reader.GetSafeString(startingIndex++);
            myTemplate.Path = reader.GetSafeString(startingIndex++);
            myTemplate.ModifiedBy = reader.GetSafeInt32(startingIndex++);
            myTemplate.CreatedBy = reader.GetSafeInt32(startingIndex++);
            myTemplate.CreatedDate = reader.GetSafeDateTime(startingIndex++);
            myTemplate.ModifiedDate = reader.GetSafeDateTime(startingIndex++);

            return myTemplate;
        }

        public CMSTemplate GetTemplate(int id)
        {
            CMSTemplate myTemplate = null;

            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {
                myTemplate = MapTemplate2(reader);

            };

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);
                //strings have to match the stored proc parameter names
            };

            _dataProvider.ExecuteCmd("dbo.CMSTemplates_SelectById", inputParamDelegate, singleRecMapper);

            return myTemplate;
        }

        private static CMSTemplate MapTemplate2(IDataReader reader)
        {
            CMSTemplate myTemplate = new CMSTemplate();
            int startingIndex = 0;

            myTemplate.Id = reader.GetSafeInt32(startingIndex++);
            myTemplate.Name = reader.GetSafeString(startingIndex++);
            myTemplate.Description = reader.GetSafeString(startingIndex++);
            myTemplate.Path = reader.GetSafeString(startingIndex++);
            myTemplate.ModifiedBy = reader.GetSafeInt32(startingIndex++);
            myTemplate.CreatedBy = reader.GetSafeInt32(startingIndex++);
            myTemplate.CreatedDate = reader.GetSafeDateTime(startingIndex++);
            myTemplate.ModifiedDate = reader.GetSafeDateTime(startingIndex++);
            return myTemplate;
        }

        public FeaturedPage GetContentByUrl(string url)
        {

            FeaturedPage page = null;

            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {

                int startingIndex = 0;

                if (set == 0)
                {

                    page = MapFeature(reader);

                }
                else if (set == 1)
                {

                    FeaturedPageInfo info = new FeaturedPageInfo();

                    info.Id = reader.GetSafeInt32(startingIndex++);
                    info.KeyName = reader.GetSafeString(startingIndex++);
                    info.TemplateKeyId = reader.GetSafeInt32(startingIndex++);
                    info.KeyType = reader.GetSafeInt32(startingIndex++);
                    info.Value = reader.GetSafeString(startingIndex++);

                    if (page.Content == null)
                    {
                        page.Content = new List<FeaturedPageInfo>();
                    }

                    page.Content.Add(info);

                }

            };

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@URL", url);
                //strings have to match the stored proc parameter names
            };

            _dataProvider.ExecuteCmd("dbo.CMSPages_GetByUrl", inputParamDelegate, singleRecMapper);

            return page;
        }

        public void UpdateTemplate(CMSTemplateUpdateRequest model, int userId)
        {
            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", model.Id);
                paramCollection.AddWithValue("@Name", model.Name);
                paramCollection.AddWithValue("@Description", model.Description);
                paramCollection.AddWithValue("@ModifiedBy", userId);
            };

            _dataProvider.ExecuteNonQuery("dbo.CMSTemplates_Update", inputParamDelegate);
        }

        public void DeleteTemplate(int id)
        {

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);
                //strings have to match the stored proc parameter names
            };

            _dataProvider.ExecuteNonQuery("dbo.CMSTemplates_Delete", inputParamDelegate);
        }


        //------------------------------------------------cms pages---------------------------------------//
        public int AddPage(CMSPageAddRequest model, int userId)
        {
            int id = 0;

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@TypeId", model.TypeId);
                paramCollection.AddWithValue("@TemplateId", model.TemplateId);
                paramCollection.AddWithValue("@UserId", userId);
                paramCollection.AddWithValue("@Name", model.Name);
                paramCollection.AddWithValue("@URL", model.Url);
                paramCollection.AddWithValue("@DateToPublish", model.DateToPublish);
                paramCollection.AddWithValue("@DateToExpire", model.DateToExpire);
                paramCollection.AddWithValue("@ParentId", model.ParentId);
                paramCollection.AddWithValue("@IsNavigation", model.IsNavigation);
                paramCollection.AddWithValue("@PageOrder", model.PageOrder);


                SqlParameter idParameter = new SqlParameter("@Id", System.Data.SqlDbType.Int);
                idParameter.Direction = System.Data.ParameterDirection.Output;

                paramCollection.Add(idParameter);
            };

            Action<SqlParameterCollection> returnParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                Int32.TryParse(paramCollection["@Id"].Value.ToString(), out id);

            };

            string proc = "dbo.CMSPages_Insert";
            _dataProvider.ExecuteNonQuery(proc, inputParamDelegate, returnParamDelegate);

            return id;
        }

        public List<CMSPage> GetPages()
        {
            List<CMSPage> list = null;

            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {
                CMSPage myPage = MapPage(reader);

                if (list == null)
                {
                    list = new List<CMSPage>();
                }

                list.Add(myPage);
            };
            Action<SqlParameterCollection> inputParamDelegate = null;

            _dataProvider.ExecuteCmd("dbo.CMSPages_SelectAll", inputParamDelegate, singleRecMapper);
            return list;
        }

        private static CMSPage MapPage(IDataReader reader)
        {
            CMSPage myPage = new CMSPage();
            int startingIndex = 0;

            myPage.Id = reader.GetSafeInt32(startingIndex++);
            myPage.TypeId = reader.GetSafeInt32(startingIndex++);
            myPage.TemplateId = reader.GetSafeInt32(startingIndex++);
            myPage.UserId = reader.GetSafeInt32(startingIndex++);
            myPage.Name = reader.GetSafeString(startingIndex++);
            myPage.Url = reader.GetSafeString(startingIndex++);
            myPage.DateToPublish = reader.GetSafeDateTime(startingIndex++);
            myPage.DateToExpire = reader.GetSafeDateTime(startingIndex++);
            myPage.DateAdded = reader.GetSafeDateTime(startingIndex++);
            myPage.DateModified = reader.GetSafeDateTime(startingIndex++);
            myPage.ParentId = reader.GetSafeInt32(startingIndex++);
            myPage.IsNavigation = reader.GetSafeBool(startingIndex++);
            myPage.PageOrder = reader.GetSafeInt32(startingIndex);

            return myPage;
        }

        public CMSPage GetPage(int id)
        {
            CMSPage myPage = null;

            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {
                myPage = new CMSPage();
                int startingIndex = 0;

                myPage.Id = reader.GetSafeInt32(startingIndex++);
                myPage.TypeId = reader.GetSafeInt32(startingIndex++);
                myPage.TemplateId = reader.GetSafeInt32(startingIndex++);
                myPage.UserId = reader.GetSafeInt32(startingIndex++);
                myPage.Name = reader.GetSafeString(startingIndex++);
                myPage.Url = reader.GetSafeString(startingIndex++);
                myPage.DateToPublish = reader.GetSafeDateTime(startingIndex++);
                myPage.DateToExpire = reader.GetSafeDateTime(startingIndex++);
                myPage.DateAdded = reader.GetSafeDateTime(startingIndex++);
                myPage.DateModified = reader.GetSafeDateTime(startingIndex++);
                myPage.ParentId = reader.GetSafeInt32(startingIndex++);
                myPage.IsNavigation = reader.GetSafeBool(startingIndex++);
                myPage.PageOrder = reader.GetSafeInt32(startingIndex);
            };

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);
                //strings have to match the stored proc parameter names
            };

            _dataProvider.ExecuteCmd("dbo.CMSPages_SelectById", inputParamDelegate, singleRecMapper);

            return myPage;
        }

        public void UpdatePage(CMSPageUpdateRequest model, int userId)
        {
            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", model.Id);
                paramCollection.AddWithValue("@TypeId", model.TypeId);
                paramCollection.AddWithValue("@TemplateId", model.TemplateId);
                paramCollection.AddWithValue("@UserId", userId);
                paramCollection.AddWithValue("@Name", model.Name);
                paramCollection.AddWithValue("@URL", model.Url);
                paramCollection.AddWithValue("@DateToPublish", model.DateToPublish);
                paramCollection.AddWithValue("@DateToExpire", model.DateToExpire);
                paramCollection.AddWithValue("@ParentId", model.ParentId);
                paramCollection.AddWithValue("@IsNavigation", model.IsNavigation);
                paramCollection.AddWithValue("@PageOrder", model.PageOrder);
            };

            _dataProvider.ExecuteNonQuery("dbo.CMSPages_Update", inputParamDelegate);
        }

        public void DeletePage(int id)
        {
            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);
                //strings have to match the stored proc parameter names
            };

            _dataProvider.ExecuteNonQuery("dbo.CMSPages_Delete", inputParamDelegate);
        }

        //-------------------------------------- cms content --------------------------------//

        public List<CMSContent> GetPageContent(int CMSPageId)
        {
            List<CMSContent> list = null;

            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {
                CMSContent myPage = MapContent(reader);

                if (list == null)
                {
                    list = new List<CMSContent>();
                }

                list.Add(myPage);
            };

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@CMSPageId", CMSPageId);
                //strings have to match the stored proc parameter names
            };

            _dataProvider.ExecuteCmd("dbo.CMSContent_SelectByPageId", inputParamDelegate, singleRecMapper);


            return list;
        }

        private static CMSContent MapContent(IDataReader reader)
        {
            CMSContent myContent = new CMSContent();
            int startingIndex = 0;

            myContent.Id = reader.GetSafeInt32(startingIndex++);
            myContent.TemplateKeyId = reader.GetSafeInt32(startingIndex++);
            myContent.Value = reader.GetSafeString(startingIndex++);
            myContent.CMSPageId = reader.GetSafeInt32(startingIndex++);

            return myContent;
        }

        public void UpdateContent(CMSContentUpdateRequest model)
        {
            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", model.Id);
                paramCollection.AddWithValue("@Value", model.Value);

            };

            _dataProvider.ExecuteNonQuery("dbo.CMSContent_Update", inputParamDelegate);
        }

        private static FeaturedPage MapFeature(IDataReader reader)
        {
            FeaturedPage myFeature = new FeaturedPage();
            int startingIndex = 0;


            myFeature.PageId = reader.GetSafeInt32(startingIndex++);
            myFeature.HtmlPath = reader.GetSafeString(startingIndex++);
            myFeature.TemplateId = reader.GetSafeInt32(startingIndex++);

            return myFeature;
        }

        public List<FeaturedPageLite> GetPageKeys(int id)
        {
            List<FeaturedPageLite> list = null;

            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {
                FeaturedPageLite myFeatureLite = MapFeatureLite(reader);

                if (list == null)
                {
                    list = new List<FeaturedPageLite>();
                }

                list.Add(myFeatureLite);
            };

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@PageId", id);
                //strings have to match the stored proc parameter names
            };

            _dataProvider.ExecuteCmd("dbo.CMSContent_SelectContentByPage", inputParamDelegate, singleRecMapper);
            return list;
        }

        private static FeaturedPageLite MapFeatureLite(IDataReader reader)
        {
            FeaturedPageLite myFeatureLite = new FeaturedPageLite();
            int startingIndex = 0;

            myFeatureLite.KeyName = reader.GetSafeString(startingIndex++);
            myFeatureLite.PageId = reader.GetSafeInt32(startingIndex++);
            myFeatureLite.Url = reader.GetSafeString(startingIndex++);
            myFeatureLite.Type = reader.GetSafeInt32(startingIndex++);
            myFeatureLite.KeyId = reader.GetSafeInt32(startingIndex++);
            myFeatureLite.Id = reader.GetSafeInt32(startingIndex++);
            myFeatureLite.TemplateKeyId = reader.GetSafeInt32(startingIndex++);
            myFeatureLite.Value = reader.GetSafeString(startingIndex++);
            myFeatureLite.CMSPageId = reader.GetSafeInt32(startingIndex++);

            return myFeatureLite;
        }

        public int PostContent(CMSContent model)
        {
            int id = 0;

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@TemplateKeyId", model.TemplateKeyId);
                paramCollection.AddWithValue("@Value", model.Value);
                paramCollection.AddWithValue("@CMSPageId", model.CMSPageId);


                SqlParameter idParameter = new SqlParameter("@Id", System.Data.SqlDbType.Int);
                idParameter.Direction = System.Data.ParameterDirection.Output;

                paramCollection.Add(idParameter);
            };

            Action<SqlParameterCollection> returnParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                Int32.TryParse(paramCollection["@Id"].Value.ToString(), out id);

            };

            string proc = "dbo.CMSContent_Insert";
            _dataProvider.ExecuteNonQuery(proc, inputParamDelegate, returnParamDelegate);

            return id;
        }

    }
}
