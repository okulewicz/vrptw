namespace VRPTWOptimizer.Utils.Validation
{
    public class ValidationError
    {
        public object ConsideredObject { get; }
        public string Description { get; }

        public ErrorCode ErrorCode { get; }
        public object ExpectedValue { get; }
        public VRPErrorType ErrorType { get; }

        public string ObjectId { get; }

        public VRPObjectType ObjectType { get; }

        public ValidationError(VRPErrorType errorType, VRPObjectType objectType, string objectId, string description, object consideredObject, ErrorCode errorCode, object expectedValue)
        {
            Description = description;
            ErrorType = errorType;
            ObjectId = objectId;
            ObjectType = objectType;
            ConsideredObject = consideredObject;
            ErrorCode = errorCode;
            ExpectedValue = expectedValue;
        }
    }
}