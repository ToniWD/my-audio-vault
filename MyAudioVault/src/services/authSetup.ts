import { client } from "@/api/client.gen";
import { tokenManager } from "./TokenManager";
import { Client, ClientOptions, createClient, createConfig } from "@/api/client";


client.interceptors.request.use(async (request) => {
    const accessToken = await tokenManager.getValidAccessToken();
    if (accessToken)
        request.headers.set('Authorization', `Bearer ${accessToken}`);

    return request;
});

// This client is used to bypass the interceptor, must be used if no access token is required
export const publicClient: Client = createClient(createConfig<ClientOptions>({ baseUrl: 'http://localhost:5204/' }));