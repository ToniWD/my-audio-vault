import { globalStyles } from "@/constants/globalStyles";
import { Text, View } from "react-native";

export default function AdminDashboard() {
    return (
        <View style={globalStyles.screen}>
            <Text style={globalStyles.title}>Admin's panel</Text>
        </View>
    );
}