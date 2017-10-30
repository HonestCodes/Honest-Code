using GSwap.Data.Providers;
using GSwap.Models.Domain;
using GSwap.Models.Requests.RegisterCookStepTwo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSwap.Data;
using System.Data;
using GSwap.Models.Requests.Cooks;
using GSwap.Models.Domain.Cooks;
using GSwap.Models.Requests.Users;
using GSwap.Models.Domain.Cuisines;
using GSwap.Models.Domain.ServicesOffered;
using GSwap.Models.Domain.Meals;

namespace GSwap.Services
{
    public class CookService : ICookService
    {
        private IDataProvider _dataProvider;

        public CookService(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public int Add(StepTwoAddRequest model, int userId)
        {
            int id = 0;

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@PreviousRestaurants", model.PreviousRestaurants);
                //strings have to match the stored proc parameter names
                paramCollection.AddWithValue("@Description", model.Description);
                paramCollection.AddWithValue("@DishesPerWeek", model.DishesPerWeek);
                paramCollection.AddWithValue("@ServicesOffered", model.ServicesOffered);
                paramCollection.AddWithValue("@CuisineStyles", model.CuisineStyles);
                paramCollection.AddWithValue("@ZipCode", model.ZipCode);
                paramCollection.AddWithValue("@WillDeliver", model.WillDeliver);
                paramCollection.AddWithValue("@UserId", userId);

                //output parameter
                SqlParameter idParameter = new SqlParameter("@Id", System.Data.SqlDbType.Int);
                idParameter.Direction = System.Data.ParameterDirection.Output;

                paramCollection.Add(idParameter);
            };


            //the delegate defined and declared as returnParamDelegate to be used below in the actual call to the DataProvider.ExecuteNonQuery
            Action<SqlParameterCollection> returnParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                Int32.TryParse(paramCollection["@Id"].Value.ToString(), out id);

            };

            string proc = "dbo.RegisteringCookStepTwo_Insert";
            _dataProvider.ExecuteNonQuery(proc, inputParamDelegate, returnParamDelegate);

            return id;

        }

        public List<Cuisine> GetAllCuisines()
        {


            List<Cuisine> list = null;

            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {
                Cuisine cuisines = MapCuisines(reader);

                if (list == null)
                {
                    list = new List<Cuisine>();
                }

                list.Add(cuisines);

            };

            Action<SqlParameterCollection> inputParamDelegate = null;


            _dataProvider.ExecuteCmd("dbo.Cuisines_SelectAll", inputParamDelegate, singleRecMapper);
            return list;
        }

        private static Cuisine MapCuisines(IDataReader reader)
        {
            Cuisine cuisines = new Cuisine();
            int startingIndex = 0;


            cuisines.Id = reader.GetSafeInt32(startingIndex++);
            cuisines.Name = reader.GetSafeString(startingIndex++);
            cuisines.UserId = reader.GetSafeInt32(startingIndex++);
            cuisines.DateAdded = reader.GetSafeDateTime(startingIndex++);
            cuisines.DateModified = reader.GetSafeDateTime(startingIndex++);

            return cuisines;
        }

        public List<MealImages> GetUserMeal(int userId)
        {
            List<MealImages> meals = null;
            Dictionary<int, List<string>> photos = null;


            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {
                int startingIndex = 0;

                if (set == 0)
                {
                    MealImages singleItem = new MealImages();

                    singleItem.Id = reader.GetSafeInt32(startingIndex++);
                    singleItem.Title = reader.GetSafeString(startingIndex++);


                    if (meals == null)
                    {
                        meals = new List<MealImages>();
                    }

                    meals.Add(singleItem);
                }

                else if (set == 1)
                {
                    int mealId = 0;
                    string path = null;
                   
                    mealId = reader.GetSafeInt32(startingIndex++);
                    path = reader.GetSafeString(startingIndex++);

                    if (photos == null)
                    {
                        photos = new Dictionary<int, List<string>>();//strings strings instead of addresses
                    }

                   

                    if (!photos.ContainsKey(mealId))
                    {
                        photos.Add(mealId,new List<string>());
                    };

                    photos[mealId].Add(path);


                }
            };

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@UserId", userId);
                //strings have to match the stored proc parameter names
            };

            _dataProvider.ExecuteCmd("dbo.CookProfile_GetMealsByUserId", inputParamDelegate, singleRecMapper);

            for (int i = 0; i < meals.Count; i++) //loops through userIds, assigning dict values of that userId key to locations property
            {
                int mealId = meals[i].Id;

                if (photos.ContainsKey(mealId))
                {
                    meals[i].Images = photos[mealId];
                }
            }

            return meals;
        }



        public List<MealInfo> GetMealTimesById(int id)
        {
            List<MealInfo> mealTime = null;

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);
                //strings have to match the stored proc parameter names
            };



            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {
                MealInfo time = MealTimeInfo(reader);

                if (mealTime == null)
                {
                    mealTime = new List<MealInfo>();
                }

                mealTime.Add(time);

            };

            _dataProvider.ExecuteCmd("dbo.Meals_GetMealInfoById", inputParamDelegate, singleRecMapper);
            return mealTime;
        }

        private static MealInfo MealTimeInfo(IDataReader reader)
        {
            MealInfo mealTime = new MealInfo();
            int startingIndex = 0;

            mealTime.Title = reader.GetSafeString(startingIndex++);
            mealTime.Description = reader.GetSafeString(startingIndex++);
            mealTime.DeliveryOption = reader.GetSafeInt32(startingIndex++);
            mealTime.Comment = reader.GetSafeString(startingIndex++);
            mealTime.StartDate = reader.GetSafeDateTime(startingIndex++);
            mealTime.EndDate = reader.GetSafeDateTime(startingIndex++);
            mealTime.UserId = reader.GetSafeInt32(startingIndex++);
            mealTime.Id = reader.GetSafeInt32(startingIndex++);

            return mealTime;
        }

        public List<CookInform> GetCookInformationById(int id)
        {

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);
                //strings have to match the stored proc parameter names
            };

            List<CookInform> information = null;

            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {
                CookInform info = MapInfo(reader);

                if (information == null)
                {
                    information = new List<CookInform>();
                }

                information.Add(info);

            };

            _dataProvider.ExecuteCmd("dbo.CookProfiles_SelectCookInformationById", inputParamDelegate, singleRecMapper);
            return information;
        }

        private static CookInform MapInfo(IDataReader reader)
        {
            CookInform information = new CookInform();
            int startingIndex = 0;

            information.PreviousRestaurants = reader.GetSafeInt32(startingIndex++);
            information.Pitch = reader.GetSafeString(startingIndex++);
            information.Bio = reader.GetSafeString(startingIndex++);
            information.Additional = reader.GetSafeString(startingIndex++);

            return information;
        }

        public List<UserProvidedService> GetUserServicesById(int id)
        {

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);
                //strings have to match the stored proc parameter names
            };

            List<UserProvidedService> services = null;

            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {
                UserProvidedService service = MapService(reader);

                if (services == null)
                {
                    services = new List<UserProvidedService>();
                }

                services.Add(service);

            };

            _dataProvider.ExecuteCmd("dbo.CookServices_SelectById", inputParamDelegate, singleRecMapper);
            return services;
        }
        private static UserProvidedService MapService(IDataReader reader)
        {
            UserProvidedService services = new UserProvidedService();
            int startingIndex = 0;

            services.Id = reader.GetSafeInt32(startingIndex++);
            services.Service = reader.GetSafeString(startingIndex++);
            return services;
        }




        public List<UserProvidedCuisines> GetCuisinesById(int id)
        {

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);
                //strings have to match the stored proc parameter names
            };

            List<UserProvidedCuisines> cuisines = null;

            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {
                UserProvidedCuisines cuisine = MapCuisine(reader);

                if (cuisines == null)
                {
                    cuisines = new List<UserProvidedCuisines>();
                }

                cuisines.Add(cuisine);

            };

            _dataProvider.ExecuteCmd("dbo.CuisineOptions_SelectById", inputParamDelegate, singleRecMapper);
            return cuisines;
        }

        private static UserProvidedCuisines MapCuisine(IDataReader reader)
        {
            UserProvidedCuisines cuisines = new UserProvidedCuisines();
            int startingIndex = 0;

            cuisines.Id = reader.GetSafeInt32(startingIndex++);
            cuisines.Name = reader.GetSafeString(startingIndex++);
            return cuisines;
        }



        public List<ServiceOffered> GetServicesProvided()
        {
            List<ServiceOffered> list = null;

            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {
                ServiceOffered services = MapServices(reader);

                if (list == null)
                {
                    list = new List<ServiceOffered>();
                }

                list.Add(services);

            };

            Action<SqlParameterCollection> inputParamDelegate = null;


            _dataProvider.ExecuteCmd("dbo.Services_SelectAll", inputParamDelegate, singleRecMapper);
            return list;
        }

        private static ServiceOffered MapServices(IDataReader reader)
        {
            ServiceOffered services = new ServiceOffered();
            int startingIndex = 0;


            services.Id = reader.GetSafeInt32(startingIndex++);
            services.Service = reader.GetSafeString(startingIndex++);
            services.UserId = reader.GetSafeInt32(startingIndex++);
            services.DateAdded = reader.GetSafeDateTime(startingIndex++);
            services.DateModified = reader.GetSafeDateTime(startingIndex++);

            return services;
        }



        public CookProfileRequest GetProfileInfoById(int id)
        {
            CookProfileRequest profileInfo = null;

            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {
                profileInfo = MapProfile(reader);
            };

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);
                //strings have to match the stored proc parameter names
            };

            _dataProvider.ExecuteCmd("dbo.CookProfiles_SelectById", inputParamDelegate, singleRecMapper);
            return profileInfo;


        }

        private static CookProfileRequest MapProfile(IDataReader reader)
        {
            CookProfileRequest profileInfo = new CookProfileRequest();
            int startingIndex = 0;


            profileInfo.FirstName = reader.GetSafeString(startingIndex++);
            profileInfo.LastName = reader.GetSafeString(startingIndex++);
            profileInfo.PhoneNumber = reader.GetSafeString(startingIndex++);
            profileInfo.Email = reader.GetSafeString(startingIndex++);
            profileInfo.FileName = reader.GetSafeString(startingIndex++);

            return profileInfo;
        }

        public CookProfile Get(int userId)
        {
            CookProfile myStepTwo = null;

            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {
                myStepTwo = MapCook_V2(reader);
            };

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@UserId", userId);
                //strings have to match the stored proc parameter names
            };

            //change this
            _dataProvider.ExecuteCmd("dbo.CookProfiles_SelectByUserId", inputParamDelegate, singleRecMapper);
            return myStepTwo;


        }


        private static CookProfile MapCook_V2(IDataReader reader)
        {
            CookProfile myStepTwo = new CookProfile();
            int startingIndex = 0;

            myStepTwo.Id = reader.GetSafeInt32(startingIndex++);
            myStepTwo.PreviousRestaurants = reader.GetSafeInt32(startingIndex++);
            myStepTwo.Bio = reader.GetSafeString(startingIndex++);
            myStepTwo.DishesPerWeek = reader.GetSafeInt32(startingIndex++);
            myStepTwo.ZipCode = reader.GetSafeInt32(startingIndex++);
            myStepTwo.WillDeliver = reader.GetSafeBool(startingIndex++);
            myStepTwo.UserId = reader.GetSafeInt32(startingIndex++);
            myStepTwo.DateAdded = reader.GetSafeDateTime(startingIndex++);
            myStepTwo.DateModified = reader.GetSafeDateTime(startingIndex++);
            return myStepTwo;
        }


        private static RegisterCookStepTwo MapCook(IDataReader reader)
        {
            RegisterCookStepTwo myStepTwo = new RegisterCookStepTwo();
            int startingIndex = 0;

            myStepTwo.Id = reader.GetSafeInt32(startingIndex++);
            myStepTwo.PreviousRestaurants = reader.GetSafeInt32(startingIndex++);
            myStepTwo.Bio = reader.GetSafeString(startingIndex++);
            myStepTwo.DishesPerWeek = reader.GetSafeInt32(startingIndex++);
            myStepTwo.ServicesOffered = reader.GetSafeString(startingIndex++);
            myStepTwo.CuisineStyles = reader.GetSafeString(startingIndex++);
            myStepTwo.ZipCode = reader.GetSafeString(startingIndex++);
            myStepTwo.WillDeliver = reader.GetSafeBool(startingIndex++);
            myStepTwo.UserId = reader.GetSafeInt32(startingIndex++);
            myStepTwo.DateAdded = reader.GetSafeDateTime(startingIndex++);
            myStepTwo.DateModified = reader.GetSafeDateTime(startingIndex++);
            return myStepTwo;
        }






        public List<RegisterCookStepTwo> Get()
        {
            List<RegisterCookStepTwo> list = null;

            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {
                RegisterCookStepTwo myStepTwo = MapCook(reader);

                if (list == null)
                {
                    list = new List<RegisterCookStepTwo>();
                }

                list.Add(myStepTwo);
            };
            Action<SqlParameterCollection> inputParamDelegate = null;


            _dataProvider.ExecuteCmd("dbo.RegisteringCookStepTwo_SelectAll", inputParamDelegate, singleRecMapper);
            return list;
        }

        public void Update(StepTwoUpdateRequest model, int UserId)
        {
            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", model.Id);
                paramCollection.AddWithValue("@PreviousRestaurants", model.PreviousRestaurants);
                //strings have to match the stored proc parameter names
                paramCollection.AddWithValue("@Description", model.Description);
                paramCollection.AddWithValue("@DishesPerWeek", model.DishesPerWeek);
                paramCollection.AddWithValue("@ServicesOffered", model.ServicesOffered);
                paramCollection.AddWithValue("@CuisineStyles", model.CuisineStyles);
                paramCollection.AddWithValue("@ZipCode", model.ZipCode);
                paramCollection.AddWithValue("@WillDeliver", model.WillDeliver);
                paramCollection.AddWithValue("@UserId", UserId);
            };

            _dataProvider.ExecuteNonQuery("dbo.RegisteringCookStepTwo_Update", inputParamDelegate);



        }

        public void Delete(int id)
        {

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);
                //strings have to match the stored proc parameter names
            };

            _dataProvider.ExecuteNonQuery("dbo.RegisteringCookStepTwo_Delete", inputParamDelegate);
        }

        public RegisterCookStepOne GetUser(int id)
        {
            RegisterCookStepOne myStepOne = new RegisterCookStepOne();

            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {

                int startingIndex = 0;

                myStepOne.Id = reader.GetSafeInt32(startingIndex++);
                myStepOne.FirstName = reader.GetSafeString(startingIndex++);
                myStepOne.LastName = reader.GetSafeString(startingIndex++);
                myStepOne.Email = reader.GetSafeString(startingIndex++);
                myStepOne.PhoneNumber = reader.GetSafeString(startingIndex++);
                myStepOne.PasswordHash = reader.GetSafeString(startingIndex++);



            };

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);
                //strings have to match the stored proc parameter names
            };

            _dataProvider.ExecuteCmd("dbo.Users_SelectById", inputParamDelegate, singleRecMapper);
            return myStepOne;

        }

        public void UpdateCookProfile(CookProfileUpdateRequest model, int userId)
        {
            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@PreviousRestaurants", model.PreviousRestaurants);
                paramCollection.AddWithValue("@Pitch", model.Pitch);
                paramCollection.AddWithValue("@Bio", model.Bio);
                paramCollection.AddWithValue("@TimeCommitmentId", model.TimeCommitmentId);
                paramCollection.AddWithValue("@Additional", model.Additional);
                paramCollection.AddWithValue("@SourceTypeId", model.SourceTypeId);
                paramCollection.AddWithValue("@Source", model.Source);
                paramCollection.AddWithValue("@DishesPerWeek", model.DishesPerWeek);
                paramCollection.AddWithValue("@WillDeliver", model.WillDeliver);
                paramCollection.AddWithValue("@UserId", userId);
            };

            _dataProvider.ExecuteNonQuery("dbo.CooksProfiles_Update", inputParamDelegate);



        }

        public void UpdateStepThree(StepThreeUpdateRequest model, int userId)
        {
            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Pitch", model.Pitch);
                paramCollection.AddWithValue("@TimeCommitmentId", model.TimeCommitmentId);
                paramCollection.AddWithValue("@UserId", userId);
            };

            _dataProvider.ExecuteNonQuery("dbo.CookProfileStepThree_Update", inputParamDelegate);
        }

        public RegisterCookStepThree GetStepThree(int id)
        {
            RegisterCookStepThree stepThree = null;

            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {
                stepThree = new RegisterCookStepThree();

                int startingIndex = 0;

                stepThree.Id = reader.GetSafeInt32(startingIndex++);
                stepThree.Pitch = reader.GetSafeString(startingIndex++);
                stepThree.TimeCommitmentId = reader.GetSafeInt32(startingIndex++);
                stepThree.DateAdded = reader.GetSafeDateTime(startingIndex++);
                stepThree.DateModified = reader.GetSafeDateTime(startingIndex++);
                stepThree.UserId = reader.GetSafeInt32(startingIndex++);
                
            };

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@UserId", id);
                //strings have to match the stored proc parameter names
            };

            _dataProvider.ExecuteCmd("dbo.CookProfileStepThree_GetById", inputParamDelegate, singleRecMapper);
            return stepThree;
        }

        public void UpdateStepFour(StepFourUpdateRequest model, int userId)
        {
            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Additional", model.Additional);
                paramCollection.AddWithValue("@SourceTypeId", model.SourceTypeId);
                paramCollection.AddWithValue("@Source", model.Source);
                paramCollection.AddWithValue("@UserId", userId);
            };

            _dataProvider.ExecuteNonQuery("dbo.CookProfileStepFour_Update", inputParamDelegate);
        }

        public RegisterCookStepFour GetStepFour(int id)
        {
            RegisterCookStepFour stepFour = null;
            Action<IDataReader, short> singleRecMapper = delegate (IDataReader reader, short set)
            {
                stepFour = new RegisterCookStepFour();

                int startingIndex = 0;

                stepFour.Id = reader.GetSafeInt32(startingIndex++);
                stepFour.Additional = reader.GetSafeString(startingIndex++);
                stepFour.SourceTypeId = reader.GetSafeInt32(startingIndex++);
                stepFour.Source = reader.GetSafeString(startingIndex++);
                stepFour.UserId = reader.GetSafeInt32(startingIndex++);


            };

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@UserId", id);
                //strings have to match the stored proc parameter names
            };

            _dataProvider.ExecuteCmd("dbo.CookProfileStepFour_GetById", inputParamDelegate, singleRecMapper);
            return stepFour;
        }

        public void UpdateCookProfile_Cooking(CookProfile_CookingUpdateRequest model, int userId)
        {

            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {


                SqlParameter idParameter = new SqlParameter("@ServicesOffered", System.Data.SqlDbType.Structured);

                if (model.Services != null && model.Services.Any())
                {

                    GSwap.Data.IntIdTable tbl = new IntIdTable(model.Services);
                    idParameter.Value = tbl;


                }
                paramCollection.Add(idParameter);

                SqlParameter idParameterTwo = new SqlParameter("@CuisineStyles", System.Data.SqlDbType.Structured);

                if (model.Cuisines != null && model.Cuisines.Any())
                {
                    GSwap.Data.IntIdTable tbl = new IntIdTable(model.Cuisines);
                    idParameterTwo.Value = tbl;


                }
                paramCollection.Add(idParameterTwo);

                paramCollection.AddWithValue("@PreviousRestaurants", model.PreviousRestaurants);
                paramCollection.AddWithValue("@Bio", model.Bio);
                paramCollection.AddWithValue("@Zipcode", model.Zipcode);
                paramCollection.AddWithValue("@DishesPerWeek", model.DishesPerWeek);
                paramCollection.AddWithValue("@WillDeliver", model.WillDeliver);
                paramCollection.AddWithValue("@UserId", userId);
            };

            _dataProvider.ExecuteNonQuery("dbo.CookProfileStepTwo_UpdateV2", inputParamDelegate);

        }

        public void updateCookStepOne(UserInfoUpdateRequest model)
        {
            Action<SqlParameterCollection> inputParamDelegate = delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@FirstName", model.FirstName);
                paramCollection.AddWithValue("@LastName", model.LastName);
                paramCollection.AddWithValue("@Email", model.Email);
                paramCollection.AddWithValue("@Number", model.Number);
                paramCollection.AddWithValue("@Id", model.Id);
            };

            _dataProvider.ExecuteNonQuery("dbo.Users_Update", inputParamDelegate);

        }



    }
}
