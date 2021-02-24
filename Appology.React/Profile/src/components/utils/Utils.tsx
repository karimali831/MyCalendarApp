// export const showAlert = (msg: string, variant?: Variant) => <Modal show={true} text={msg} variant={variant} />
export const rootUrl: string = process.env.NODE_ENV === "development" ? "http://localhost:53822" : window.location.origin;
export const appUrl: string = "http://localhost:3000";
export const appPathUrl: string = "/account/settings";