namespace Company.Core.Template.Application.Common.CustomMediator;

// Representa a próxima ação na cadeia do pipeline. Pode ser o próximo behavior ou o handler final.
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();